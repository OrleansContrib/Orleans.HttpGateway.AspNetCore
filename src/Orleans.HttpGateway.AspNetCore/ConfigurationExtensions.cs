using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans.HttpGateway.AspNetCore.ParameterBinding;

namespace Orleans.HttpGateway.AspNetCore
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddOrleansHttpGateway(this IServiceCollection services, Action<OrleansHttpGatewayOptions> configure)
        {

            services.AddRouting();
            services.Configure<OrleansHttpGatewayOptions>(options =>
            {
                configure?.Invoke(options);
            });


            return services;
        }

        public static IApplicationBuilder UseOrleansHttpGateway(this IApplicationBuilder app)
        {
            var routeBuilder = new RouteBuilder(app);

            var options = app.ApplicationServices.GetService<IOptions<OrleansHttpGatewayOptions>>();

            if (options.Value.JsonSerializerSettings == null)
            {
                //get the serializer settings from service container
                options.Value.JsonSerializerSettings = app.ApplicationServices.GetService<JsonSerializerSettings>()
                    ??new JsonSerializerSettings();
            }

            options.Value.JsonSerializerSettings.Converters.Add(new ImmutableConverter());

            routeBuilder.MapRoute(
                "{grainInterface}/{grainId}/{grainMethod}",
                part =>
                {
                    part.UseMiddleware<OrleansHttpGatewayMiddleware>(options);
                }
            );

            var routes = routeBuilder.Build();


            app.UseRouter(routes);

            return app;
        }
    }

}