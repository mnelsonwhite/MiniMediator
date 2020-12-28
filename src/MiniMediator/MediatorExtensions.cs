using System;
using System.Threading.Tasks;
using MiniMediator.Abstractions;

namespace MiniMediator
{
    public static class MediatorExtensions
    {
        public static IMediator Publish<TMessage>(this IMediator mediator) where TMessage : new()
            => mediator.Publish(new TMessage());
        public static IMediator Subscribe<TMessage>(this IMediator mediator, Action<TMessage> subscription)
            => mediator.Subscribe(subscription, out _);
        public static IMediator SubscribeAsync<TMessage>(this IMediator mediator, Func<TMessage, Task> subscription)
            => mediator.SubscribeAsync(subscription, out _);
        public static IMediator Subscribe<TMessage>(this IMediatorSubscribable<TMessage> mediator, Action<TMessage> subscription)
            => mediator.Subscribe(subscription, out _);
        public static IMediator SubscribeAsync<TMessage>(this IMediatorSubscribable<TMessage> mediator, Func<TMessage, Task> subscription)
            => mediator.SubscribeAsync(subscription, out _);
        public static IMediator Subscribe<TMessage>(this IMediator mediator, IMessageHandler<TMessage> handler)
            => mediator.Subscribe<TMessage>(message => handler.Handle(message));
        public static IMediator SubscribeAsync<TMessage>(this IMediator mediator, IMessageHandlerAsync<TMessage> handler)
            => mediator.SubscribeAsync<TMessage>(message => handler.Handle(message));
        public static IMediator Subscribe<TMessage>(this IMediator mediator, IFilteredMessageHandler<TMessage> handler)
            => mediator
                .Where(handler.Predicate)
                .Subscribe(message => handler.Handle(message));
        public static IMediator SubscribeAsync<TMessage>(this IMediator mediator, IFilteredMessageHandlerAsync<TMessage> handler)
            => mediator
                .Where(handler.Predicate)
                .SubscribeAsync(message => handler.Handle(message));
    }
}
