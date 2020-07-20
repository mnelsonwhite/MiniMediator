using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MiniMediator
{
    public class Mediator
    {
        private readonly IDictionary<Type, BehaviourSubject<object>> observers;
        private readonly ILogger? _logger;
        private readonly LogLevel? _loggingLevel;

        public Mediator()
        {
            observers = new Dictionary<Type, BehaviourSubject<object>>();
        }

        public Mediator(ILogger logger, LogLevel? loggingLevel = null) : this()
        {
            _logger = logger;
            _loggingLevel = loggingLevel;
        }

        public Mediator Publish<TMessage>() where TMessage : new ()
        {
            return Publish(new TMessage());
        }

        public virtual Mediator Publish<TMessage>(TMessage message)
        {
            if (_loggingLevel.HasValue) _logger?.Log(
                _loggingLevel.Value,
                "Publishing {@Message}",
                message
            );
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

        public Mediator SubscribeAsync<TMessage>(Func<TMessage, Task> subscription)
        {
            return SubscribeAsync(subscription, out _);
        }

        public virtual Mediator Subscribe<TMessage>(Action<TMessage> subscription, out IDisposable disposable)
        {
            if (_loggingLevel.HasValue) _logger?.Log(
                _loggingLevel.Value,
                "Subscribing {TMessage}",
                typeof(TMessage)
            );

            if (!observers.ContainsKey(typeof(TMessage)))
            {
                observers.Add(typeof(TMessage), new BehaviourSubject<object>());
            }

            // Currently the Cast extension method is the only reason the System.Reactive pacakge is needed.
            // it looks non-trivial to implement.
            disposable = observers[typeof(TMessage)].Cast<TMessage>().Subscribe(message => {
                try
                {
                    subscription(message);
                }
                catch(Exception ex)
                {
                    Publish(new ExceptionMessage<TMessage>(ex, message));
                    Publish(new ExceptionMessage(ex, message!));
                }
            });

            return this;
        }

        public virtual Mediator SubscribeAsync<TMessage>(Func<TMessage,Task> subscription, out IDisposable disposable)
        {
            if (_loggingLevel.HasValue) _logger?.Log(
                _loggingLevel.Value,
                "Subscribing {TMessage}",
                typeof(TMessage)
            );

            if (!observers.ContainsKey(typeof(TMessage)))
            {
                observers.Add(typeof(TMessage), new BehaviourSubject<object>());
            }

            disposable = observers[typeof(TMessage)]
                .Cast<TMessage>()
                .Select(message => Observable
                    .FromAsync(() => subscription.Invoke(message))
                    .Catch<Unit,Exception>(ex => {
                        Publish(new ExceptionMessage<TMessage>(ex, message));
                        Publish(new ExceptionMessage(ex, message!));
                        return Observable.Return(Unit.Default);
                    })
                )
                .Concat()
                .Subscribe();

            return this;
        }

        public Mediator Subscribe<TMessage>(IMessageHandler<TMessage> handler)
        {
            if (_loggingLevel.HasValue) _logger?.Log(
                _loggingLevel.Value,
                "Subscribing {Handler}",
                handler
            );

            return Subscribe<TMessage>(message => handler.Handle(message));
        }

        public Mediator SubscribeAsync<TMessage>(IMessageHandlerAsync<TMessage> handler)
        {
            if (_loggingLevel.HasValue) _logger?.Log(
                _loggingLevel.Value,
                "Subscribing {Handler}",
                handler
            );

            return SubscribeAsync<TMessage>(message => handler.Handle(message));
        }

        public Mediator Subscribe<THandler,TMessage>() where THandler : IMessageHandler<TMessage>, new()
        {
            return Subscribe(new THandler());
        }

        public Mediator SubscribeAsync<THandler, TMessage>() where THandler : IMessageHandlerAsync<TMessage>, new()
        {
            return SubscribeAsync(new THandler());
        }
    }
}
