using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MiniMediator;
using MiniMediator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{

    public static partial class ContainerExtensions
    {
        private static Type[] _handlerGenericTypes = new[] {
            typeof(IFilteredMessageHandler<>),
            typeof(IFilteredMessageHandlerAsync<>),
            typeof(IMessageHandler<>),
            typeof(IMessageHandlerAsync<>)
        };

        public static IServiceCollection AddMediator(this IServiceCollection services)
        {
            return AddMediator(services, options => { });
        }

        public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorOptions> options)
        {
            var optionsInstance = new MediatorOptions();
            
            options(optionsInstance);
            RegisterAssembly(services, optionsInstance);

            var handlerTypes = services
                .SelectMany(d => {
                    var interfaces = d.ServiceType.GetInterfaces().Select(iface => (type: d.ServiceType, iface));
                    return d.ServiceType.IsInterface
                        ? interfaces.Concat(new[] { (type: d.ServiceType, iface: d.ServiceType) })
                        : interfaces;
                })
                .Where(serviceType =>
                    { return serviceType.iface.IsGenericType && _handlerGenericTypes.Contains(serviceType.iface.GetGenericTypeDefinition()); }
                )
                .Select(serviceType => (serviceType.type, messageType: serviceType.iface.GetGenericArguments().Single()))
                .Distinct()
                .ToArray();

            services.Add(
                new ServiceDescriptor(
                    typeof(IMediator),
                    provider => {
                        var mediator = new ContainerMediator(
                            provider,
                            handlerTypes,
                            provider.GetService<ILogger<IMediator>>(),
                            optionsInstance.LoggingLevel
                        );

                        return mediator;
                    },
                    optionsInstance.Lifetime
                )
            );

            return services;
        }

        private static void RegisterAssembly(IServiceCollection services, MediatorOptions options)
        {
            var handlerTypes = options.Assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && type
                    .GetInterfaces()
                    .Any(iface =>
                        iface.IsGenericType && _handlerGenericTypes.Contains(iface.GetGenericTypeDefinition())
                    )
                )
                .ToArray();

            foreach (var handlerType in handlerTypes)
            {
                services.TryAdd(new ServiceDescriptor(handlerType, handlerType, options.HandlerLifetime));
            }
        }
    }
}
