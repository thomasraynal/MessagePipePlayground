using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MessagePipePlayground.Sandbox
{
    public enum AsyncPublishStrategy
    {
        Parallel, Sequential
    }

    public enum InstanceLifetime
    {
        Singleton, Scoped
    }

    public enum HandlingSubscribeDisposedPolicy
    {
        Ignore, Throw
    }

    internal static class HandlingSubscribeDisposedPolicyExtensions
    {
        public static IDisposable Handle(this HandlingSubscribeDisposedPolicy policy, string name)
        {
            if (policy == HandlingSubscribeDisposedPolicy.Throw)
            {
                throw new ObjectDisposedException(name);
            }
            return DisposableBag.Empty;
        }
    }

    public sealed class MessagePipeOptions
    {
        /// <summary>PublishAsync</summary>
        public AsyncPublishStrategy DefaultAsyncPublishStrategy { get; set; }

        /// <summary>For diagnostics usage, enable MessagePipeDiagnosticsInfo.CapturedStacktraces; default is false.</summary>
        public bool EnableCaptureStackTrace { get; set; }

        public HandlingSubscribeDisposedPolicy HandlingSubscribeDisposedPolicy { get; set; }

        public InstanceLifetime InstanceLifetime { get; set; }

        public MessagePipeOptions()
        {
            this.DefaultAsyncPublishStrategy = AsyncPublishStrategy.Parallel;
            this.InstanceLifetime = InstanceLifetime.Singleton;
            this.EnableCaptureStackTrace = false;
            this.HandlingSubscribeDisposedPolicy = HandlingSubscribeDisposedPolicy.Ignore;
            this.autoregistrationAssemblies = null;
            this.autoregistrationTypes = null;

        }

        internal Assembly[]? autoregistrationAssemblies;
        internal Type[]? autoregistrationTypes;

        public void SetAutoRegistrationSearchAssemblies(params Assembly[] assemblies)
        {
            autoregistrationAssemblies = assemblies;
        }

        public void SetAutoRegistrationSearchTypes(params Type[] types)
        {
            autoregistrationTypes = types;
        }

        void ValidateFilterType(Type type, Type filterType)
        {
            if (!filterType.IsAssignableFrom(type))
            {
                throw new ArgumentException($"{type.FullName} is not {filterType.Name}");
            }
        }
    }
}
