using Microsoft.Extensions.Logging;
using MiniMediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Extensions.DependencyInjection
{

    public static partial class ContainerExtensions
    {
        internal class ContainerMediator : Mediator
        {
            private int _addedHandlers = 0;
            private readonly IServiceProvider _provider;
            private readonly IReadOnlyCollection<(Type type, Type messageType)> _handlers;

            public ContainerMediator(
                IServiceProvider provider,
                IReadOnlyCollection<(Type type, Type messageType)> handlers,
                ILogger logger) : base(logger)
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
