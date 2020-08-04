using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiniMediator
{
    public class Mediator : IMediator
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

        public virtual IMediator Publish<TMessage>(TMessage message)
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

        public virtual IMediator Subscribe<TMessage>(Action<TMessage> subscription, out IDisposable disposable)
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
            disposable = observers[typeof(TMessage)]
                .Cast<TMessage>()
                .Subscribe(message => {
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

        public virtual IMediator SubscribeAsync<TMessage>(Func<TMessage,Task> subscription, out IDisposable disposable)
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

        public IMediatorSubscribable<TMessage> Where<TMessage>(Func<TMessage, bool> predicate)
        {
            return new WhereSubscribable<TMessage>(this, predicate);
        }

        private class WhereSubscribable<TMessage> : IMediatorSubscribable<TMessage>
        {
            private readonly Mediator _mediator;
            private readonly Func<TMessage, bool> _predicate;

            public WhereSubscribable(Mediator mediator, Func<TMessage, bool> predicate)
            {
                _mediator = mediator;
                _predicate = predicate;
            }
            public IMediator Subscribe(Action<TMessage> subscription, out IDisposable disposable)
            {
                if (_mediator._loggingLevel.HasValue) _mediator._logger?.Log(
                _mediator._loggingLevel.Value,
                "Subscribing {TMessage}",
                typeof(TMessage)
            );

                if (!_mediator.observers.ContainsKey(typeof(TMessage)))
                {
                    _mediator.observers.Add(typeof(TMessage), new BehaviourSubject<object>());
                }

                // Currently the Cast extension method is the only reason the System.Reactive pacakge is needed.
                // it looks non-trivial to implement.
                disposable = _mediator
                    .observers[typeof(TMessage)]
                    .Cast<TMessage>()
                    .Where(_predicate)
                    .Subscribe(message => {
                        try
                        {
                            subscription(message);
                        }
                        catch (Exception ex)
                        {
                            _mediator.Publish(new ExceptionMessage<TMessage>(ex, message));
                            _mediator.Publish(new ExceptionMessage(ex, message!));
                        }
                    });

                return _mediator;
            }

            public IMediator SubscribeAsync(Func<TMessage, Task> subscription, out IDisposable disposable)
            {
                if (_mediator._loggingLevel.HasValue) _mediator._logger?.Log(
                    _mediator._loggingLevel.Value,
                    "Subscribing {TMessage}",
                    typeof(TMessage)
                );

                if (!_mediator.observers.ContainsKey(typeof(TMessage)))
                {
                    _mediator.observers.Add(typeof(TMessage), new BehaviourSubject<object>());
                }

                disposable = _mediator.observers[typeof(TMessage)]
                    .Cast<TMessage>()
                    .Where(_predicate)
                    .Select(message => Observable
                        .FromAsync(() => subscription.Invoke(message))
                        .Catch<Unit, Exception>(ex => {
                            _mediator.Publish(new ExceptionMessage<TMessage>(ex, message));
                            _mediator.Publish(new ExceptionMessage(ex, message!));
                            return Observable.Return(Unit.Default);
                        })
                    )
                    .Concat()
                    .Subscribe();

                return _mediator;
            }
        }
    }
}
