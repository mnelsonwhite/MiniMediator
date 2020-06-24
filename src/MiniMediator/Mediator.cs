using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace MiniMediator
{
    public class Mediator
    {
        /// <summary>
        /// Callback on publish. Used for logging
        /// </summary>
        public event EventHandler<IPublishEventArgs>? OnPublished;
        private readonly IDictionary<Type, BehaviourSubject<object>> observers;
        
        public Mediator()
        {
            observers = new Dictionary<Type, BehaviourSubject<object>>();
        }

        public Mediator Publish<TMessage>() where TMessage : new ()
        {
            return Publish(new TMessage());
        }

        public Mediator Publish<TMessage>(TMessage message)
        {
            OnPublished?.Invoke(this, new PublishEventArgs(message!));
            if (message == null) throw new ArgumentNullException(nameof(message));

            var type = typeof(TMessage);

            var messageObservers = observers.Where(kv => kv.Key == type || kv.Key.IsAssignableFrom(type)).ToArray();
            foreach (var pair in messageObservers)
            {
                pair.Value.OnNext(message!);
            }

            if (messageObservers.Length == 0)
            {
                observers.Add(typeof(TMessage), new BehaviourSubject<object>(message));
            }

            return this;
        }

        public Mediator Subscribe<TMessage>(Action<TMessage> subscription)
        {
            return Subscribe(subscription, out _);
        }

        public Mediator Subscribe<TMessage>(Action<TMessage> subscription, out IDisposable disposable)
        {
            if (!observers.ContainsKey(typeof(TMessage)))
            {
                observers.Add(typeof(TMessage), new BehaviourSubject<object>());
            }

            disposable = observers[typeof(TMessage)].Cast<TMessage>().Subscribe(subscription);
            return this;
        }

        public Mediator Subscribe<TMessage>(IMessageHandler<TMessage> handler)
        {
            return Subscribe<TMessage>(message => handler.Handle(message));
        }

        public Mediator Subscribe<THandler,TMessage>() where THandler : IMessageHandler<TMessage>, new()
        {
            var handler = new THandler();
            return Subscribe<TMessage>(message => handler.Handle(message));
        }
    }
}
