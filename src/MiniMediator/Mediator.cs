using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace MiniMediator
{
    public class Mediator
    {
        protected readonly IDictionary<Type, Subject<object>> observers;

        public Mediator()
        {
            observers = new Dictionary<Type, Subject<object>>();
        }

        public Mediator Publish<T>() where T : new ()
        {
            return Publish(new T());
        }

        public Mediator Publish<T>(T message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var type = typeof(T);
            foreach (var pair in observers.Where(kv => kv.Key == type || kv.Key.IsAssignableFrom(type)))
            {
                pair.Value.OnNext(message!);
            }

            return this;
        }

        public Mediator Subscribe<T>(Action<T> subscription)
        {
            return Subscribe(subscription, out _);
        }

        public Mediator Subscribe<TEvent>(Action<TEvent> subscription, out IDisposable disposable)
        {
            if (!observers.ContainsKey(typeof(TEvent)))
            {
                observers.Add(typeof(TEvent), new Subject<object>());
            }

            disposable = observers[typeof(TEvent)].Cast<TEvent>().Subscribe(subscription);
            return this;
        }
    }
}
