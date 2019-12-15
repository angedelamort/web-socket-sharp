using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace WebSocketSharp
{
    public static class WebSocketManagerMiddlewareExtensions
    {
        private static readonly List<Type> WebSocketTypes = InitWebSocketHandler();

        public static IServiceCollection AddWebSocket(this IServiceCollection services)
        {
            services.AddTransient<WebSocketDictionary>();

            foreach (var type in WebSocketTypes)
                services.AddSingleton(type);

            return services;
        }

        public static IApplicationBuilder MapWebSockets(this IApplicationBuilder builder)
        {
            var serviceScopeFactory = builder.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            var serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;

            foreach (var type in WebSocketTypes)
            {
                var routeAttribute = type.GetCustomAttribute<RouteAttribute>();
                string route;
                if (routeAttribute != null)
                {
                    route = routeAttribute.Template;
                }
                else
                {
                    route = type.Name;
                    var suffix = "Handler";
                    if (typeof(WebSocketController).IsAssignableFrom(type))
                        suffix = "Controller";

                    if (route.EndsWith(suffix))
                        route = route.Substring(0, route.Length - suffix.Length);
                }

                builder.Map(route, app =>
                {
                    var handler = serviceProvider.GetService(type);
                    app.UseMiddleware<WebSocketManagerMiddleware>(handler);
                });
            }

            return builder;
        }

        private static List<Type> InitWebSocketHandler()
        {
            var items = new List<Type>();

            var exportedTypes = Assembly.GetEntryAssembly()?.ExportedTypes;
            if (exportedTypes != null)
            {
                foreach (var type in exportedTypes)
                {
                    if (typeof(WebSocketHandler).IsAssignableFrom(type))
                        items.Add(type);
                }
            }

            return items;
        }
    }
}
