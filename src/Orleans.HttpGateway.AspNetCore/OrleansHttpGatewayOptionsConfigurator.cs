using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans.HttpGateway.AspNetCore.ParameterBinding;

namespace Orleans.HttpGateway.AspNetCore
{

    internal class OrleansHttpGatewayOptionsConfigurator : IConfigureOptions<OrleansHttpGatewayOptions>
    {
        private readonly IServiceProvider _services;

        public OrleansHttpGatewayOptionsConfigurator(IServiceProvider services)
        {
            _services = services;
        }

        public void Configure(OrleansHttpGatewayOptions options)
        {
            if (options.JsonSerializerSettings == null)
            {
                //get the serializer settings from service container
                options.JsonSerializerSettings = _services.GetService<JsonSerializerSettings>()
                    ?? new JsonSerializerSettings();
            }

            if (!options.JsonSerializerSettings.Converters.OfType<ImmutableConverter>().Any())
            {
                options.JsonSerializerSettings.Converters.Add(new ImmutableConverter());
            }
        }
    }

}