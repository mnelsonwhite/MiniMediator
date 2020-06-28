using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace MiniMediator
{
    public class Mediator
    {
        private readonly IDictionary<Type, BehaviourSubject<object>> observers;
        private readonly ILogger? _logger;
        
        public Mediator()
        {
            observers = new Dictionary<Type, BehaviourSubject<object>>();
        }

        public Mediator(ILogger logger) : this()
        {
            _logger = logger;
        }

        public Mediator Publish<TMessage>() where TMessage : new ()
        {
            return Publish(new TMessage());
        }

        public virtual Mediator Publish<TMessage>(TMessage message)
        {
            _logger?.LogDebug("Publishing {@Message}", message);
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

        public virtual Mediator Subscribe<TMessage>(Action<TMessage> subscription, out IDisposable disposable)
        {
            _logger?.LogDebug("Subscribing {TMessage}", typeof(TMessage));
            if (!observers.ContainsKey(typeof(TMessage)))
            {
                observers.Add(typeof(TMessage), new BehaviourSubject<object>());
            }

            // Currently the Cast extension method is the only reason the System.Reactive pacakge is needed.
            // it looks non-trivial to implement.
            disposable = observers[typeof(TMessage)].Cast<TMessage>().Subscribe(subscription);
            return this;
        }

        public Mediator Subscribe<TMessage>(IMessageHandler<TMessage> handler)
        {
            _logger?.LogDebug("Subscribing {Handler}", handler);
            return Subscribe<TMessage>(message => handler.Handle(message));
        }

        public Mediator Subscribe<THandler,TMessage>() where THandler : IMessageHandler<TMessage>, new()
        {
            var handler = new THandler();
            return Subscribe<TMessage>(message => handler.Handle(message));
        }
    }
}
