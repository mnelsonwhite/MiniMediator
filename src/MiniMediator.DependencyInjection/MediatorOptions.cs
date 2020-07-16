using Microsoft.Extensions.Logging;
using MiniMediator;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public class MediatorOptions
    {
        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Singleton;
        public ServiceLifetime HandlerLifetime { get; set; } = ServiceLifetime.Singleton;
        public List<Assembly> Assemblies { get; } = new List<Assembly>();
        public LogLevel? LoggingLevel { get; } = null;
    }
}
