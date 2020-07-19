using System;

namespace MiniMediator
{
    public class ExceptionMessage<TMessage>
    {
        public ExceptionMessage(Exception exception, TMessage message)
        {
            Exception = exception;
            Message = message;
        }

        public Exception Exception { get; }
        public TMessage Message { get; }
    }

    public class ExceptionMessage : ExceptionMessage<object>
    {
        public ExceptionMessage(Exception exception, object message) : base (exception, message)
        {

        }
    }
}
