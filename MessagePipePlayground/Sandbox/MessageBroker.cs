using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MessagePipePlayground.Sandbox
{

    public sealed class MessageBroker<TKey, TMessage> : IPublisher<TKey, TMessage>, ISubscriber<TKey, TMessage> where TKey : notnull
    {
        readonly MessageBrokerCore<TKey, TMessage> core;


        public MessageBroker(MessageBrokerCore<TKey, TMessage> core)
        {
            this.core = core;
        }

        public void Publish(TKey key, TMessage message)
        {
            core.Publish(key, message);
        }

        public IDisposable Subscribe(TKey key, IMessageHandler<TMessage> handler, params MessageHandlerFilter<TMessage>[] filters)
        {
            return core.Subscribe(key, handler);
        }
    }

    public sealed class MessageBrokerCore<TKey, TMessage> : IDisposable
    where TKey : notnull
    {
        readonly Dictionary<TKey, HandlerHolder> handlerGroup;
        readonly MessagePipeDiagnosticsInfo diagnotics;
        readonly HandlingSubscribeDisposedPolicy handlingSubscribeDisposedPolicy;
        readonly object gate;
        bool isDisposed;

        public MessageBrokerCore(MessagePipeDiagnosticsInfo diagnotics, MessagePipeOptions options)
        {
            this.handlerGroup = new Dictionary<TKey, HandlerHolder>();
            this.diagnotics = diagnotics;
            this.handlingSubscribeDisposedPolicy = options.HandlingSubscribeDisposedPolicy;
            this.gate = new object();
        }

        public void Publish(TKey key, TMessage message)
        {
            IMessageHandler<TMessage>?[] handlers;
            lock (gate)
            {
                if (!handlerGroup.TryGetValue(key, out var holder))
                {
                    return;
                }
                handlers = holder.GetHandlers();
            }

            for (int i = 0; i < handlers.Length; i++)
            {
                handlers[i]?.Handle(message);
            }
        }

        public IDisposable Subscribe(TKey key, IMessageHandler<TMessage> handler)
        {
            lock (gate)
            {
                if (isDisposed) return handlingSubscribeDisposedPolicy.Handle(nameof(MessageBrokerCore<TKey, TMessage>));

                if (!handlerGroup.TryGetValue(key, out var holder))
                {
                    handlerGroup[key] = holder = new HandlerHolder(this);
                }

                return holder.Subscribe(key, handler);
            }
        }

        public void Dispose()
        {
            lock (gate)
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    foreach (var handlers in handlerGroup.Values)
                    {
                        handlers.Dispose();
                    }
                }
            }
        }

        // similar as Keyless-MessageBrokerCore but require to remove when key is empty on Dispose
        sealed class HandlerHolder : IDisposable, IHandlerHolderMarker
        {
            readonly FreeList<IMessageHandler<TMessage>> handlers;
            readonly MessageBrokerCore<TKey, TMessage> core;

            public HandlerHolder(MessageBrokerCore<TKey, TMessage> core)
            {
                this.handlers = new FreeList<IMessageHandler<TMessage>>();
                this.core = core;
            }

            public IMessageHandler<TMessage>?[] GetHandlers() => handlers.GetValues();

            public IDisposable Subscribe(TKey key, IMessageHandler<TMessage> handler)
            {
                var subscriptionKey = handlers.Add(handler);
                var subscription = new Subscription(key, subscriptionKey, this);
                core.diagnotics.IncrementSubscribe(this, subscription);
                return subscription;
            }

            public void Dispose()
            {
                lock (core.gate)
                {
                    if (handlers.TryDispose(out var count))
                    {
                        core.diagnotics.RemoveTargetDiagnostics(this, count);
                    }
                }
            }

            sealed class Subscription : IDisposable
            {
                bool isDisposed;
                readonly TKey key;
                readonly int subscriptionKey;
                readonly HandlerHolder holder;

                public Subscription(TKey key, int subscriptionKey, HandlerHolder holder)
                {
                    this.key = key;
                    this.subscriptionKey = subscriptionKey;
                    this.holder = holder;
                }

                public void Dispose()
                {
                    if (!isDisposed)
                    {
                        isDisposed = true;
                        lock (holder.core.gate)
                        {
                            if (!holder.core.isDisposed)
                            {
                                holder.handlers.Remove(subscriptionKey, false);
                                holder.core.diagnotics.DecrementSubscribe(holder, this);
                                if (holder.handlers.GetCount() == 0)
                                {
                                    holder.core.handlerGroup.Remove(key);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public sealed class MessageBroker<TMessage> : IPublisher<TMessage>, ISubscriber<TMessage>
    {
        readonly MessageBrokerCore<TMessage> core;

        public MessageBroker(MessageBrokerCore<TMessage> core)
        {
            this.core = core;
        }

        public void Publish(TMessage message)
        {
            core.Publish(message);
        }

        public IDisposable Subscribe(IMessageHandler<TMessage> handler, params MessageHandlerFilter<TMessage>[] filters)
        {
            return core.Subscribe(handler);
        }
    }

    public sealed class MessageBrokerCore<TMessage> : IDisposable, IHandlerHolderMarker
    {
        readonly FreeList<IMessageHandler<TMessage>> handlers;
        readonly MessagePipeDiagnosticsInfo diagnotics;
        readonly HandlingSubscribeDisposedPolicy handlingSubscribeDisposedPolicy;
        readonly object gate = new object();
        bool isDisposed;

        public MessageBrokerCore(MessagePipeDiagnosticsInfo diagnotics, MessagePipeOptions options)
        {
            this.handlers = new FreeList<IMessageHandler<TMessage>>();
            this.handlingSubscribeDisposedPolicy = options.HandlingSubscribeDisposedPolicy;
            this.diagnotics = diagnotics;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Publish(TMessage message)
        {
            var array = handlers.GetValues();
            for (int i = 0; i < array.Length; i++)
            {
                array[i]?.Handle(message);
            }
        }

        public IDisposable Subscribe(IMessageHandler<TMessage> handler)
        {
            lock (gate)
            {
                if (isDisposed) return handlingSubscribeDisposedPolicy.Handle(nameof(MessageBrokerCore<TMessage>));

                var subscriptionKey = handlers.Add(handler);
                var subscription = new Subscription(this, subscriptionKey);
                diagnotics.IncrementSubscribe(this, subscription);
                return subscription;
            }
        }

        public void Dispose()
        {
            lock (gate)
            {
                // Dispose is called when scope is finished.
                if (!isDisposed && handlers.TryDispose(out var count))
                {
                    isDisposed = true;
                    diagnotics.RemoveTargetDiagnostics(this, count);
                }
            }
        }

        sealed class Subscription : IDisposable
        {
            bool isDisposed;
            readonly MessageBrokerCore<TMessage> core;
            readonly int subscriptionKey;

            public Subscription(MessageBrokerCore<TMessage> core, int subscriptionKey)
            {
                this.core = core;
                this.subscriptionKey = subscriptionKey;
            }

            public void Dispose()
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    lock (core.gate)
                    {
                        if (!core.isDisposed)
                        {
                            core.handlers.Remove(subscriptionKey, true);
                            core.diagnotics.DecrementSubscribe(core, this);
                        }
                    }
                }
            }
        }
    }
}
