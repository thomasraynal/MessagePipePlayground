using System;
using System.Collections.Generic;
using System.Text;

namespace MessagePipePlayground
{
    public class SomeEvent
    {
        public SomeEvent()
        {
            EventId = Guid.NewGuid();
        }

        public Guid EventId { get; }

        public override string ToString()
        {
            return EventId.ToString();
        }
    }
}
