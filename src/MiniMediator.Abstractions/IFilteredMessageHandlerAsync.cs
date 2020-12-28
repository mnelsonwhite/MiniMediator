using System;

namespace MiniMediator.Abstractions
{
    public interface IFilteredMessageHandlerAsync<TMessage> : IMessageHandlerAsync<TMessage>
    {
        Func<TMessage, bool> Predicate { get; }
    }
}
