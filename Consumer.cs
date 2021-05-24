using MessagePipePlayground.Sandbox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MessagePipePlayground
{
    public class Consumer<MyEvent> : IDisposable where MyEvent : new()
    {
        private readonly ISubscriber<MyEvent> _subscriber;
        private readonly IDisposable _disposable;

        public Consumer(ISubscriber<MyEvent> subscriber)
        {
            var bag = DisposableBag.CreateBuilder();

            _subscriber = subscriber;

            _subscriber.Subscribe(@event => Trace.WriteLine(@event)).AddTo(bag);

            _disposable = bag.Build();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

 
    }
}
