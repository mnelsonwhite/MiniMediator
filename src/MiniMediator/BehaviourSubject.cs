using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace MiniMediator
{
    /// <summary>
    /// System.Reactive.Subjects.BehaviorSubject doesn't work as I would hope.
    /// Ideally it would only invoke OnNext on new subscriptions when a values has already been set,
    /// instead of having to set the initial value when constructing the subject.
    /// 
    /// This makes a big difference since it avoid having to use a null initial value or dummy value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BehaviourSubject<T> : SubjectBase<T>
    {
        private readonly object _gate = new object();

        private ImmutableList<IObserver<T>> _observers;
        private bool _isStopped;
        private Settable<T> _value;
        private Exception _exception = default!;
        private bool _isDisposed;

        public BehaviourSubject(T value)
        {
            _value = new Settable<T>(value);
            _observers = ImmutableList<IObserver<T>>.Empty;
        }

        public BehaviourSubject()
        {
            _value = new Settable<T>();
            _observers = ImmutableList<IObserver<T>>.Empty;
        }

        public override bool HasObservers => _observers?.Count > 0;

        public override bool IsDisposed
        {
            get
            {
                lock (_gate)
                {
                    return _isDisposed;
                }
            }
        }

        public bool TryGetValue(out T value)
        {
            lock (_gate)
            {
                if (_isDisposed)
                {
                    value = default!;
                    return false;
                }

                if (_exception != null)
                {
                    throw _exception;
                }

                value = _value.Value;
                return true;
            }
        }

        public override void OnCompleted()
        {
            var os = default(IObserver<T>[]);
            lock (_gate)
            {
                CheckDisposed();

                if (!_isStopped)
                {
                    os = _observers.ToArray();
                    _observers = ImmutableList<IObserver<T>>.Empty;
                    _isStopped = true;
                }
            }

            if (os != null)
            {
                foreach (var o in os)
                {
                    o.OnCompleted();
                }
            }
        }

        public override void OnError(Exception error)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            var os = default(IObserver<T>[]);
            lock (_gate)
            {
                CheckDisposed();

                if (!_isStopped)
                {
                    os = _observers.ToArray();
                    _observers = ImmutableList<IObserver<T>>.Empty;
                    _isStopped = true;
                    _exception = error;
                }
            }

            if (os != null)
            {
                foreach (var o in os)
                {
                    o.OnError(error);
                }
            }
        }

        public override void OnNext(T value)
        {
            var os = default(IObserver<T>[]);
            lock (_gate)
            {
                CheckDisposed();

                if (!_isStopped)
                {
                    _value.Value = value;
                    os = _observers.ToArray();
                }
            }

            if (os != null)
            {
                foreach (var o in os)
                {
                    o.OnNext(value);
                }
            }
        }

        public override IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            var ex = default(Exception);

            lock (_gate)
            {
                CheckDisposed();

                if (!_isStopped)
                {
                    _observers = _observers.Add(observer);
                    if (_value.IsSet) observer.OnNext(_value.Value);
                    return new Subscription(this, observer);
                }

                ex = _exception;
            }

            if (ex != null)
            {
                observer.OnError(ex);
            }
            else
            {
                observer.OnCompleted();
            }

            return new EmptyDisposable();
        }

        public override void Dispose()
        {
            lock (_gate)
            {
                _isDisposed = true;
                _observers = null!;
                _value = default!;
                _exception = null!;
            }
        }

        private void CheckDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(string.Empty);
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly BehaviourSubject<T> _subject;
            private IObserver<T> _observer;

            public Subscription(BehaviourSubject<T> subject, IObserver<T> observer)
            {
                _subject = subject;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null)
                {
                    lock (_subject._gate)
                    {
                        if (!_subject._isDisposed && _observer != null)
                        {
                            _subject._observers = _subject._observers.Remove(_observer);
                            _observer = null!;
                        }
                    }
                }
            }
        }
    }
}
