using MiniMediator.Abstractions;

namespace MiniMediator
{
    internal class PublishEventArgs : IPublishEventArgs
    {
        internal PublishEventArgs(object message)
        {
            Message = message;
        }

        public object Message { get; }
    }
}
