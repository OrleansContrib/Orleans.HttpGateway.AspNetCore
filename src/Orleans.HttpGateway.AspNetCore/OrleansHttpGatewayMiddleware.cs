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
    internal class OrleansHttpGatewayMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IGrainTypeProvider _grainTypeProvider;
        private readonly IGrainReferenceProvider _grainReferenceProvider;
        private readonly IDynamicGrainMethodInvoker _grainInvoker;
        private readonly JsonSerializer _serializer;


        public OrleansHttpGatewayMiddleware(RequestDelegate next, IGrainFactory grainFactory,
            IOptions<OrleansHttpGatewayOptions> config)
        {
            if (grainFactory == null) throw new ArgumentNullException(nameof(grainFactory));
            if (config == null) throw new ArgumentNullException(nameof(config));

            this._next = next;

            _grainTypeProvider =
                new CachedGrainTypeProvider(
                    new CompositeGrainTypeProvider(
                        config.Value.Assemblies.Select(x => new AssemblyBasedGrainTypeProvider(x))
                    ));

            _serializer = JsonSerializer.Create(config.Value.JsonSerializerSettings);
            _grainReferenceProvider = new ExpressionBasedGrainReferenceProvider(grainFactory);
            _grainInvoker = new DynamicGrainMethodInvoker(
                new IParameterBinder[]
                {
                    new JsonBodyParameterBinder(_serializer),   //order is important here, we expect application/json requests
                    new NamedQueryStringParameterBinder(_serializer),
                });
          
        }

        public async Task Invoke(HttpContext context)
        {
            var data = context.GetRouteData();
            var grainRouteValues = new GrainRouteValues(data);

            var grainType = GetGrainType(grainRouteValues.GrainInterface);
            var grain = GetGrainReference(grainType, grainRouteValues.GrainId);
            var result = await _grainInvoker.Invoke(grainType, grain, grainRouteValues, context);

            

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            using (var writer = new StreamWriter(context.Response.Body))
            {
                _serializer.Serialize(writer, result);

                // Perf: call FlushAsync to call WriteAsync on the stream with any content left in the TextWriter's
                // buffers. This is better than just letting dispose handle it (which would result in a synchronous
                // write).
                await writer.FlushAsync();
            }
        }

        private object GetGrainReference(Type grainType, string grainId)
        {
            return _grainReferenceProvider.GetGrainReference(grainType, grainId);
        }

        private Type GetGrainType(string typename)
        {
            return _grainTypeProvider.GetGrainType(typename);
        }
    }

}