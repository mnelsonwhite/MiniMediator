using System;

namespace MiniMediator
{
    public interface IPublishEventArgs
    {
        object Message { get; }
    }
}
