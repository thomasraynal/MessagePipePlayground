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

        public List<MyEvent> Events { get; } = new List<MyEvent>();

        public Consumer(ISubscriber<MyEvent> subscriber)
        {
            var bag = DisposableBag.CreateBuilder();

            _subscriber = subscriber;

            _subscriber.Subscribe(Events.Add).AddTo(bag);

            _disposable = bag.Build();
        }

        public Consumer()
        {
            Events = new List<MyEvent>();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

 
    }
}
