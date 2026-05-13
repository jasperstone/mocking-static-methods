using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Persistence.AzureStorage;
using Xunit;

namespace Orleans.Persistence.AzureStorage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ThrowsInvalidOperationExceptionWhenOptionsMonitorMissing()
        {
            var provider = new RecordingServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() => AzureTableGrainStorage.AzureTableGrainStorageFactory.Create(provider, "TestProvider"));

            Assert.Contains("IOptionsMonitor", exception.Message);
            Assert.Contains(nameof(AzureTableStorageOptions), exception.Message);
            var requestedType = Assert.Single(provider.RequestedTypes);
            Assert.Equal(typeof(IOptionsMonitor<AzureTableStorageOptions>), requestedType);
        }

        private sealed class RecordingServiceProvider : IServiceProvider
        {
            public List<Type> RequestedTypes { get; } = new();

            public object? GetService(Type serviceType)
            {
                RequestedTypes.Add(serviceType);
                return null;
            }
        }
    }
}
