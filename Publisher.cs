using MessagePipePlayground.Sandbox;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessagePipePlayground
{
    public class Publisher<MyEvent> where MyEvent: new()
    {
        private readonly IPublisher<MyEvent> _publisher;

        public Publisher(IPublisher<MyEvent> publisher)
        {
            this._publisher = publisher;
        }

        public void Send()
        {
            this._publisher.Publish(new MyEvent());
        }
    }
}
