using System;
using System.Collections.Generic;
using System.Text;

namespace MessagePipePlayground.Sandbox
{
    public interface ISubscriber<TMessage>
    {
        IDisposable Subscribe(IMessageHandler<TMessage> handler, params MessageHandlerFilter<TMessage>[] filters);
    }

    public interface ISubscriber<TKey, TMessage>
    where TKey : notnull
    {
        IDisposable Subscribe(TKey key, IMessageHandler<TMessage> handler, params MessageHandlerFilter<TMessage>[] filters);
    }
}
