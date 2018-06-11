using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Orleans.HttpGateway.AspNetCore
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddOrleansHttpGateway(this IServiceCollection services, Action<OrleansHttpGatewayOptions> configure)
        {

            services.AddRouting();
            services.AddSingleton<IConfigureOptions<OrleansHttpGatewayOptions>, OrleansHttpGatewayOptionsConfigurator>();
            services.Configure<OrleansHttpGatewayOptions>(options =>
            {
                configure?.Invoke(options);
            });

            return services;
        }

        public static IApplicationBuilder UseOrleansHttpGateway(this IApplicationBuilder app)
        {
            var routeBuilder = new RouteBuilder(app);         

            routeBuilder.MapMiddlewareRoute("{grainInterface}/{grainId}/{grainMethod}", part =>
            {
                part.UseMiddleware<OrleansHttpGatewayMiddleware>();
            });

            var routes = routeBuilder.Build();

            app.UseRouter(routes);

            return app;
        }
    }

}