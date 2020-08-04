using System;
using System.Threading.Tasks;

namespace MiniMediator
{
    public interface IMediatorSubscribable<TMessage>
    {
        IMediator Subscribe(Action<TMessage> subscription, out IDisposable disposable);
        IMediator SubscribeAsync(Func<TMessage, Task> subscription, out IDisposable disposable);
    }
}
