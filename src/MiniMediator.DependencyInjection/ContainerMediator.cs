using Microsoft.Extensions.Logging;
using MiniMediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Microsoft.Extensions.DependencyInjection
{

    public static partial class ContainerExtensions
    {
        private static readonly MethodInfo _subscribeMethod = typeof(MediatorExtensions).GetMethods().Where(
            method =>
            {
                if (
                    method.Name == nameof(MediatorExtensions.Subscribe) &&
                    method.IsGenericMethod &&
                    method.IsStatic &&
                    method.IsPublic &&
                    method.GetParameters().Length == 2
                )
                {
                    return method.GetParameters()
                        .Where(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                        .Count() == 1;
                }

                return false;
            }
        ).Single();

        private static readonly MethodInfo _subscribeAsyncMethod = typeof(MediatorExtensions).GetMethods().Where(
            method =>
            {
                if (
                    method.Name == nameof(MediatorExtensions.SubscribeAsync) &&
                    method.IsGenericMethod &&
                    method.IsStatic &&
                    method.IsPublic &&
                    method.GetParameters().Length == 2
                )
                {
                    return method.GetParameters()
                        .Where(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(IMessageHandlerAsync<>))
                        .Count() == 1;
                }

                return false;
            }
        ).Single();

        internal class ContainerMediator : Mediator
        {
            private int _addedHandlers = 0;
            private readonly IServiceProvider _provider;
            private readonly IReadOnlyCollection<(Type type, Type messageType)> _handlers;

            public ContainerMediator(
                IServiceProvider provider,
                IReadOnlyCollection<(Type type, Type messageType)> handlers,
                ILogger<IMediator> logger,
                LogLevel? loggingLevel = null) : base(logger, loggingLevel)
            {
                _provider = provider;
                _handlers = handlers;
            }

            public override IMediator Publish<TMessage>(TMessage message)
            {
                if (Interlocked.CompareExchange(ref _addedHandlers, 1, 0) == 0)
                {
                    AddHandlers();
                }

                return base.Publish(message);
            }

            private void AddHandlers()
            {
                foreach (var handler in _handlers)
                {
                    var genericMethod = GetMethodInfo(handler.type).MakeGenericMethod(handler.messageType);
                    var handlerInstance = _provider.GetService(handler.type);
                    genericMethod.Invoke(null, new object[] { this, handlerInstance });
                }
            }

            private MethodInfo GetMethodInfo(Type handlerType)
            {
                if (handlerType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IMessageHandler<>)))
                {
                    return _subscribeMethod!;
                }
                else if (handlerType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IMessageHandlerAsync<>)))
                {
                    return _subscribeAsyncMethod!;
                }

                throw new InvalidCastException("Unsupported handler type");
            }
        }
    }
}
