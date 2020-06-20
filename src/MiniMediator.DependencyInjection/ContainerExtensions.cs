using Microsoft.Extensions.DependencyInjection.Extensions;
using MiniMediator;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{

    public static class ContainerExtensions
    {
        public static IServiceCollection AddMediator(this IServiceCollection services)
        {
            return AddMediator(services, options => { });
        }

        public static IServiceCollection AddMediator(this IServiceCollection services, Action<MediatorOptions> options)
        {
            var optionsInstance = new MediatorOptions();
            options(optionsInstance);

            var handlerTypes = optionsInstance.Assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type
                    .GetInterfaces()
                    .Any(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                )
                .ToArray();

            foreach(var handlerType in handlerTypes)
            {
                services.TryAddTransient(handlerType);
            }

            services.TryAddTransient(provider => services);
            services.Add(
                new ServiceDescriptor(
                    typeof(Mediator), provider => bulidMediator(provider, optionsInstance),
                    optionsInstance.Lifetime
                )
            );

            return services;
        }

        private static Mediator bulidMediator(IServiceProvider provider, MediatorOptions options)
        {
            var mediator = new Mediator();
            if (options.PublishEventHandler != null) mediator.OnPublished += options.PublishEventHandler;

            var services = provider.GetService<IServiceCollection>();
            var subscribeMethod = typeof(Mediator).GetMethods().Where(
                method =>
                {
                    if (
                        method.Name != "Subscribe" ||
                        !method.IsGenericMethod ||
                        method.IsStatic ||
                        !method.IsPublic ||
                        method.GetParameters().Length != 1
                    ) return false;

                    var parameterType = method.GetParameters().Single().ParameterType;
                    return parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(IMessageHandler<>);
                }
            ).Single();

            var handlerTypes = services
                .SelectMany(descriptor => descriptor.ServiceType.GetInterfaces().Select(iface => (type: descriptor.ServiceType, iface)))
                .Where(serviceType => serviceType.iface.IsGenericType && serviceType.iface.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                .Select(serviceType => (serviceType.type, message: serviceType.iface.GetGenericArguments().Single()));

            foreach (var handler in handlerTypes)
            {
                var genericMethod = subscribeMethod.MakeGenericMethod(handler.message);
                var handlerInstance = provider.GetService(handler.type);
                genericMethod.Invoke(mediator, new object[] { handlerInstance });
            }

            return mediator;
        }
    }
}
