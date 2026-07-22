using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Storage;
using Orleans.Runtime;
using Xunit;

namespace Orleans.Storage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ServiceProviderNull_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => AzureTableGrainStorageFactory.Create(null!, "test"));
            Assert.Equal("services", ex.ParamName);
        }

        [Fact]
        public void Create_NameNull_ThrowsInvalidOperationException()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            Assert.ThrowsAny<Exception>(() => AzureTableGrainStorageFactory.Create(services, null!));
        }

        [Fact]
        public void Create_NoIOptionsMonitorRegistered_ThrowsInvalidOperationException()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            var ex = Assert.Throws<InvalidOperationException>(() => AzureTableGrainStorageFactory.Create(services, "test"));
            Assert.Contains("IOptionsMonitor", ex.Message);
            Assert.Contains("AzureTableStorageOptions", ex.Message);
        }

        [Fact]
        public void Create_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange - create minimal service provider that has the required service
            var services = new ServiceCollection();
            services.AddOptions<AzureTableStorageOptions>()
                    .Configure<AzureTableStorageOptions>(o => o.TableName = "test");
            services.AddSingleton<ClusterOptions>(new ClusterOptions());
            var serviceProvider = services.BuildServiceProvider();

            // Act - should call GetRequiredService successfully
            _ = AzureTableGrainStorageFactory.Create(serviceProvider, "test");
        }
    }
}
