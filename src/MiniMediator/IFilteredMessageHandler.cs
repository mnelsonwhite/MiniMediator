using System;

namespace MiniMediator
{
    public interface IFilteredMessageHandler<TMessage> : IMessageHandler<TMessage>
    {
        Func<TMessage,bool> Predicate { get; }
    }
}
