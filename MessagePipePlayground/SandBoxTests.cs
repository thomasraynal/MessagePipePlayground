using MessagePipePlayground.Sandbox;
using NUnit.Framework;
using System;

namespace MessagePipePlayground
{
    [TestFixture]
    public class SandBoxTests
    {

        [Test]
        public void ShouldCreateASubscriberAndAPublisher()
        {
            var messagePipeOptions = new MessagePipeOptions();
            var messagePipeDiagnosticsInfo = new MessagePipeDiagnosticsInfo(messagePipeOptions);
            var messageBrokerCore = new MessageBrokerCore<SomeEvent>(messagePipeDiagnosticsInfo, messagePipeOptions);
            var messageBroker = new MessageBroker<SomeEvent>(messageBrokerCore);

            var consumer = new Consumer<SomeEvent>(messageBroker);
            var publisher = new Publisher<SomeEvent>(messageBroker);

            publisher.Send();

            Assert.True(consumer.Events.Count == 1);
        }

    }
}
