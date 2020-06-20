using System;

namespace MiniMediator
{
    internal class PublishEventArgs : IPublishEventArgs
    {
        internal PublishEventArgs(object message, Type subscriberType)
        {
            Message = message;
            SubscriberType = subscriberType;
        }
        public object Message { get; }
        public Type SubscriberType { get; }
    }
}
