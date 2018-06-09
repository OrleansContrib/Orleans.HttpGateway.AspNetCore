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
    public static class OrleansHttpGatewayMiddleware
    {
        private static IGrainTypeProvider _grainTypeProvider;
        private static IGrainReferenceProvider _grainReferenceProvider;
        private static IDynamicGrainMethodInvoker _grainInvoker;
        private static JsonSerializer _serializer;

        public static void Intialize(IGrainFactory grainFactory,
            IOptions<OrleansHttpGatewayOptions> config)
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

        }

        public static async Task Invoke(HttpContext context)
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