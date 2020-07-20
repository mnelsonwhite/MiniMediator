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
        private static readonly MethodInfo _subscribeMethod = typeof(Mediator).GetMethods().Where(
            method =>
            {
                if (
                    method.Name != nameof(Mediator.Subscribe) ||
                    !method.IsGenericMethod ||
                    method.IsStatic ||
                    !method.IsPublic ||
                    method.GetParameters().Length != 1
                ) return false;

                var parameterType = method.GetParameters().Single().ParameterType;
                return parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(IMessageHandler<>);
            }
        ).Single();

        private static readonly MethodInfo _subscribeAsyncMethod = typeof(Mediator).GetMethods().Where(
            method =>
            {
                if (
                    method.Name != nameof(Mediator.SubscribeAsync) ||
                    !method.IsGenericMethod ||
                    method.IsStatic ||
                    !method.IsPublic ||
                    method.GetParameters().Length != 1
                ) return false;

                var parameterType = method.GetParameters().Single().ParameterType;
                return parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(IMessageHandlerAsync<>);
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
                ILogger<Mediator> logger,
                LogLevel? loggingLevel = null) : base(logger, loggingLevel)
            {
                _provider = provider;
                _handlers = handlers;
            }

            public override Mediator Publish<TMessage>(TMessage message)
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
                    genericMethod.Invoke(this, new object[] { handlerInstance });
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
