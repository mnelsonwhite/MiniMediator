using System;

namespace MiniMediator
{
    public interface IPublishEventArgs
    {
        object Message { get; }
        Type SubscriberType { get; }
    }
}
