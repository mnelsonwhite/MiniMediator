using System;
using System.Collections.Generic;
using System.Text;

namespace MiniMediator
{
    public interface IMessageHandler<TMessage>
    {
        void Handle(TMessage message);
    }
}
