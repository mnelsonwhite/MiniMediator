using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;

namespace MiniMediator
{
    public class Mediator
    {
        protected readonly IDictionary<Type, Subject<object>> observers;

        public Mediator()
        {
            observers = new Dictionary<Type, Subject<object>>();
        }

        public Mediator Publish<TMessage>() where TMessage : new ()
        {
            return Publish(new TMessage());
        }

        public Mediator Publish<TMessage>(TMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var type = typeof(TMessage);
            foreach (var pair in observers.Where(kv => kv.Key == type || kv.Key.IsAssignableFrom(type)))
            {
                pair.Value.OnNext(message!);
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
                observers.Add(typeof(TMessage), new Subject<object>());
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
