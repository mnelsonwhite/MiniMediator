using Microsoft.Extensions.DependencyInjection.Extensions;
using MiniMediator;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IContainerExtensions
    {
        public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
        {
            var handlerTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type
                    .GetInterfaces()
                    .Any(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                )
                .ToArray();

            foreach(var handlerType in handlerTypes)
            {
                services.AddTransient(handlerType);
            }

            services.TryAddTransient(provider => services);
            services.AddSingleton(bulidMediator);
            return services;
        }

        private static Mediator bulidMediator(IServiceProvider provider)
        {
            var mediator = new Mediator();
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
