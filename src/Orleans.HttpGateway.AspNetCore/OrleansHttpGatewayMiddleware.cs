using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans.HttpGateway.AspNetCore.GrainTypeProviders;
using Orleans.HttpGateway.AspNetCore.ParameterBinding;

namespace Orleans.HttpGateway.AspNetCore
{
    public class OrleansHttpGatewayMiddleware
    {
        private readonly IGrainTypeProvider _grainTypeProvider;
        private readonly IGrainReferenceProvider _grainReferenceProvider;
        private readonly IDynamicGrainMethodInvoker _grainInvoker;
        private readonly JsonSerializer _serializer;
        private readonly RequestDelegate _next;

        public OrleansHttpGatewayMiddleware(RequestDelegate next,IOptions<OrleansHttpGatewayOptions> config, IGrainFactory grainFactory)
        {
            if (grainFactory == null) throw new ArgumentNullException(nameof(grainFactory));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var compositeProvider = new CompositeGrainTypeProvider(config.Value.Assemblies.Select(x => new AssemblyBasedGrainTypeProvider(x)));

            _grainTypeProvider = new CachedGrainTypeProvider(compositeProvider);
            _serializer = JsonSerializer.Create(config.Value.JsonSerializerSettings);
            _grainReferenceProvider = new ExpressionBasedGrainReferenceProvider(grainFactory);
            _grainInvoker = new DynamicGrainMethodInvoker(
                new IParameterBinder[]
                {
                    new JsonBodyParameterBinder(_serializer),   //order is important here, we expect application/json requests
                    new NamedQueryStringParameterBinder(_serializer),
                });
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var data = context.GetRouteData();
            var grainRouteValues = new GrainRouteValues(data);
            var grainType = _grainTypeProvider.GetGrainType(grainRouteValues.GrainInterface);
            var grain = _grainReferenceProvider.GetGrainReference(grainType, grainRouteValues.GrainId);
            var result = await _grainInvoker.Invoke(grainType, grain, grainRouteValues, context);

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            using (var writer = new StreamWriter(context.Response.Body))
            {
                _serializer.Serialize(writer, result);
                await writer.FlushAsync();
            }
        }
    }
}