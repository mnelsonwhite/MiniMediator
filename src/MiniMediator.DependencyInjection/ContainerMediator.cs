using Microsoft.Extensions.Logging;
using MiniMediator;
using MiniMediator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Microsoft.Extensions.DependencyInjection
{

    public static partial class ContainerExtensions
    {
        private static readonly Type _filteredMessageHanderType = typeof(IFilteredMessageHandler<>);
        private static readonly Type _filteredMessageHandlerAsyncType = typeof(IFilteredMessageHandlerAsync<>);
        private static readonly Type _messageHandlerType = typeof(IMessageHandler<>);
        private static readonly Type _messageHandlerAsyncType = typeof(IMessageHandlerAsync<>);

        private static readonly MethodInfo _subscribeFilteredMethod = typeof(MediatorExtensions)
            .GetMethods()
            .Where(
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
                            .Where(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == _filteredMessageHanderType)
                            .Count() == 1;
                    }

                    return false;
                }
            )
            .Single();

        private static readonly MethodInfo _subscribeAsyncFilteredMethod = typeof(MediatorExtensions)
            .GetMethods()
            .Where(
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
                            .Where(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == _filteredMessageHandlerAsyncType)
                            .Count() == 1;
                    }

                    return false;
                }
            )
            .Single();
        private static readonly MethodInfo _subscribeMethod = typeof(MediatorExtensions)
            .GetMethods()
            .Where(
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
                            .Where(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == _messageHandlerType)
                            .Count() == 1;
                    }

                    return false;
                }
            )
            .Single();

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
                        .Where(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == _messageHandlerAsyncType)
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
                foreach (var (type, messageType) in _handlers)
                {
                    var genericMethod = GetMethodInfo(type).MakeGenericMethod(messageType);
                    var handlerInstance = _provider.GetService(type)!;
                    genericMethod.Invoke(null, new object[] { this, handlerInstance });
                }
            }

            private static MethodInfo GetMethodInfo(Type handlerType)
            {
                if (CheckHandlerType(handlerType, _filteredMessageHanderType))
                {
                    return _subscribeFilteredMethod!;
                }
                else if (CheckHandlerType(handlerType, _filteredMessageHandlerAsyncType))
                {
                    return _subscribeAsyncFilteredMethod!;
                }
                else if (CheckHandlerType(handlerType, _messageHandlerType))
                {
                    return _subscribeMethod!;
                }
                else if (CheckHandlerType(handlerType, _messageHandlerAsyncType))
                {
                    return _subscribeAsyncMethod!;
                }

                throw new InvalidCastException("Unsupported handler type");
            }

            private static bool CheckHandlerType(Type handlerType, Type handlerInterfaceType)
            {
                return
                    handlerType.IsGenericType &&
                    handlerType.GetGenericTypeDefinition() == handlerInterfaceType ||
                    handlerType.GetInterfaces().Any(
                        t => t.IsGenericType && t.GetGenericTypeDefinition() == handlerInterfaceType
                    );
            }
        }
    }
}
