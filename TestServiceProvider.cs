using System;
using System.Collections.Generic;
using System.Text;

namespace MessagePipePlayground
{
    public class TestServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
