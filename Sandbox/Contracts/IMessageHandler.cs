using System;
using System.Collections.Generic;
using System.Text;

namespace MessagePipePlayground.Sandbox
{
    public interface IMessageHandler<TMessage>
    {
        void Handle(TMessage message);
    }
}
