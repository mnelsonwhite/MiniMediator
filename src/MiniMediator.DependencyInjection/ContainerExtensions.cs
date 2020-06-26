using Microsoft.Extensions.DependencyInjection.Extensions;
using MiniMediator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;

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

            RegisterAssembly(services, optionsInstance);

            var handlerTypes = services
                .SelectMany(descriptor => descriptor.ServiceType.GetInterfaces().Select(iface => (type: descriptor.ServiceType, iface)))
                .Where(serviceType => serviceType.iface.IsGenericType && serviceType.iface.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                .Select(serviceType => (serviceType.type, messageType: serviceType.iface.GetGenericArguments().Single()))
                .ToArray();

            services.Add(
                new ServiceDescriptor(
                    typeof(Mediator),
                    provider => new ContainerMediator(
                        provider,
                        handlerTypes
                    ),
                    optionsInstance.Lifetime
                )
            );

            return services;
        }

        private static void RegisterAssembly(IServiceCollection services, MediatorOptions options)
        {
            var handlerTypes = options.Assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type
                    .GetInterfaces()
                    .Any(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                )
                .ToArray();


            foreach (var handlerType in handlerTypes)
            {
                services.TryAdd(new ServiceDescriptor(handlerType, handlerType, options.HandlerLifetime));
            }
        }

        internal class ContainerMediator : Mediator
        {
            private object _lock = new object();
            private bool _addedHandlers;
            private readonly IServiceProvider _provider;
            private readonly IReadOnlyCollection<(Type type, Type messageType)> _handlers;

            public ContainerMediator(
                IServiceProvider provider,
                IReadOnlyCollection<(Type type, Type messageType)> handlers)
            {
                _provider = provider;
                _handlers = handlers;
            }

            public override Mediator Publish<TMessage>(TMessage message)
            {
                lock(_lock)
                {
                    if (!_addedHandlers)
                    {
                        _addedHandlers = true;
                        AddHandlers();
                    }
                }

                base.Publish(message);
                return this;
            }

            private void AddHandlers()
            {
                var subscribeMethod = typeof(Mediator).GetMethods().Where(
                    method =>
                    {
                        if (
                            method.Name != nameof(Subscribe) ||
                            !method.IsGenericMethod ||
                            method.IsStatic ||
                            !method.IsPublic ||
                            method.GetParameters().Length != 1
                        ) return false;

                        var parameterType = method.GetParameters().Single().ParameterType;
                        return parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(IMessageHandler<>);
                    }
                ).Single();

                foreach (var handler in _handlers)
                {
                    var genericMethod = subscribeMethod.MakeGenericMethod(handler.messageType);
                    var handlerInstance = _provider.GetService(handler.type);
                    genericMethod.Invoke(this, new object[] { handlerInstance });
                }
            }
        }
    }
}
