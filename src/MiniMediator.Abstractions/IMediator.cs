using System;
using System.Threading.Tasks;

namespace MiniMediator.Abstractions
{
    public interface IMediator
    {
        IMediator Publish<TMessage>(TMessage message);
        IMediator Subscribe<TMessage>(Action<TMessage> subscription, out IDisposable disposable);
        IMediator SubscribeAsync<TMessage>(Func<TMessage, Task> subscription, out IDisposable disposable);
        IMediatorSubscribable<TMessage> Where<TMessage>(Func<TMessage, bool> predicate);
    }
}
