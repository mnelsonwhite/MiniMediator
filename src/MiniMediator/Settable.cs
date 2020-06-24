namespace MiniMediator
{
    internal class Settable<TValue>
    {
        private TValue _value = default!;

        public Settable()
        {

        }

        public Settable(TValue value)
        {
            _value = value;
            IsSet = true;
        }

        public bool IsSet { get; private set; } = false;
        public TValue Value
        {
            get => _value;
            set
            {
                _value = value;
                IsSet = true;
            }
        }
    }
}
