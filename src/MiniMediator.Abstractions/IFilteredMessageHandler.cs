using System;

namespace MiniMediator.Abstractions
{
    public interface IFilteredMessageHandler<TMessage> : IMessageHandler<TMessage>
    {
        Func<TMessage, bool> Predicate { get; }
    }
}
