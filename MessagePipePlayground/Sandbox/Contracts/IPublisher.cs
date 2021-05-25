using System;
using System.Collections.Generic;
using System.Text;

namespace MessagePipePlayground.Sandbox
{
    public interface IPublisher<TMessage>
    {
        void Publish(TMessage message);
    }

    public interface IPublisher<TKey, TMessage>
    where TKey : notnull
    {
        void Publish(TKey key, TMessage message);
    }

}
