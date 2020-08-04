using System;

namespace MiniMediator
{
    public interface IFilteredMessageHandlerAsync<TMessage> : IMessageHandlerAsync<TMessage>
    {
        Func<TMessage,bool> Predicate { get; }
    }
}
