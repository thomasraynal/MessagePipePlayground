﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessagePipePlayground.Sandbox
{
    public static partial class SubscriberExtensions
    {
        // pubsub-keyless-sync

        public static IDisposable Subscribe<TMessage>(this ISubscriber<TMessage> subscriber, Action<TMessage> handler, params MessageHandlerFilter<TMessage>[] filters)
        {
            return subscriber.Subscribe(new AnonymousMessageHandler<TMessage>(handler), filters);
        }

        public static IDisposable Subscribe<TMessage>(this ISubscriber<TMessage> subscriber, Action<TMessage> handler, Func<TMessage, bool> predicate, params MessageHandlerFilter<TMessage>[] filters)
        {
            var predicateFilter = new PredicateFilter<TMessage>(predicate);
            filters = (filters.Length == 0)
                ? new[] { predicateFilter }
                : ArrayUtil.ImmutableAdd(filters, predicateFilter);

            return subscriber.Subscribe(new AnonymousMessageHandler<TMessage>(handler), filters);
        }

        public static IObservable<TMessage> AsObservable<TMessage>(this ISubscriber<TMessage> subscriber, params MessageHandlerFilter<TMessage>[] filters)
        {
            return new ObservableSubscriber<TMessage>(subscriber, filters);
        }

    }

    internal sealed class AnonymousMessageHandler<TMessage> : IMessageHandler<TMessage>
    {
        readonly Action<TMessage> handler;

        public AnonymousMessageHandler(Action<TMessage> handler)
        {
            this.handler = handler;
        }

        public void Handle(TMessage message)
        {
            handler.Invoke(message);
        }
    }


    internal sealed class ObservableSubscriber<TKey, TMessage> : IObservable<TMessage>
        where TKey : notnull
    {
        readonly TKey key;
        readonly ISubscriber<TKey, TMessage> subscriber;
        readonly MessageHandlerFilter<TMessage>[] filters;

        public ObservableSubscriber(TKey key, ISubscriber<TKey, TMessage> subscriber, MessageHandlerFilter<TMessage>[] filters)
        {
            this.key = key;
            this.subscriber = subscriber;
            this.filters = filters;
        }

        public IDisposable Subscribe(IObserver<TMessage> observer)
        {
            return subscriber.Subscribe(key, new ObserverMessageHandler<TMessage>(observer), filters);
        }
    }

    internal sealed class ObservableSubscriber<TMessage> : IObservable<TMessage>
    {
        readonly ISubscriber<TMessage> subscriber;
        readonly MessageHandlerFilter<TMessage>[] filters;

        public ObservableSubscriber(ISubscriber<TMessage> subscriber, MessageHandlerFilter<TMessage>[] filters)
        {
            this.subscriber = subscriber;
            this.filters = filters;
        }

        public IDisposable Subscribe(IObserver<TMessage> observer)
        {
            return subscriber.Subscribe(new ObserverMessageHandler<TMessage>(observer), filters);
        }
    }

    internal sealed class ObserverMessageHandler<TMessage> : IMessageHandler<TMessage>
    {
        readonly IObserver<TMessage> observer;

        public ObserverMessageHandler(IObserver<TMessage> observer)
        {
            this.observer = observer;
        }

        public void Handle(TMessage message)
        {
            observer.OnNext(message);
        }
    }
}
