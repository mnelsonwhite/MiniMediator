using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace SimpleMediator
{
    public class Mediator
    {
        protected readonly IDictionary<Type, Subject<object>> observers;

        public Mediator()
        {
            observers = new Dictionary<Type, Subject<object>>();
        }

        public Mediator Send<T>(T message)
        {
            foreach (var pair in observers.Where(kv => kv.Key.IsAssignableFrom(typeof(T))))
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
