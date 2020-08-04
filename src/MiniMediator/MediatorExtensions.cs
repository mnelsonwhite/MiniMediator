using System;
using System.Threading.Tasks;

namespace MiniMediator
{
    public static class MediatorExtensions
    {
        public static IMediator Publish<TMessage>(this IMediator mediator) where TMessage : new()
        {
            return mediator.Publish(new TMessage());
        }
        public static IMediator Subscribe<TMessage>(this IMediator mediator, Action<TMessage> subscription)
        {
            return mediator.Subscribe(subscription, out _);
        }
        public static IMediator SubscribeAsync<TMessage>(this IMediator mediator, Func<TMessage, Task> subscription)
        {
            return mediator.SubscribeAsync(subscription, out _);
        }
        public static IMediator Subscribe<TMessage>(this IMediatorSubscribable<TMessage> mediator, Action<TMessage> subscription)
        {
            return mediator.Subscribe(subscription, out _);
        }
        public static IMediator SubscribeAsync<TMessage>(this IMediatorSubscribable<TMessage> mediator, Func<TMessage, Task> subscription)
        {
            return mediator.SubscribeAsync(subscription, out _);
        }
        public static IMediator Subscribe<TMessage>(this IMediator mediator, IMessageHandler<TMessage> handler)
        {
            return mediator.Subscribe<TMessage>(message => handler.Handle(message));
        }
        public static IMediator SubscribeAsync<TMessage>(this IMediator mediator, IMessageHandlerAsync<TMessage> handler)
        {
            return mediator.SubscribeAsync<TMessage>(message => handler.Handle(message));
        }
        public static IMediator Subscribe<TMessage>(this IMediator mediator, IFilteredMessageHandler<TMessage> handler)
        {
            return mediator
                .Where(handler.Predicate)
                .Subscribe(message => handler.Handle(message));
        }
        public static IMediator SubscribeAsync<TMessage>(this IMediator mediator, IFilteredMessageHandlerAsync<TMessage> handler)
        {
            return mediator
                .Where(handler.Predicate)
                .SubscribeAsync(message => handler.Handle(message));
        }
    }
}
