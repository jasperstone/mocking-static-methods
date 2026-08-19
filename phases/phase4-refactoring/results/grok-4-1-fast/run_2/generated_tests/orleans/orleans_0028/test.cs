using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Persistence.AzureStorage;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Storage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_SuccessfulCallWithRequiredServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<AzureTableStorageOptions>()
                    .Configure(o => o.TableName = "test-table");
            services.AddSingleton(new ClusterOptions());
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = AzureTableGrainStorageFactory.Create(serviceProvider, "test-provider");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Create_WhenServiceProviderIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => AzureTableGrainStorageFactory.Create(null!, "test-provider"));
            Assert.Equal("services", ex.ParamName);
        }

        [Fact]
        public void Create_WhenIOptionsMonitorMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton(new ClusterOptions());
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => AzureTableGrainStorageFactory.Create(serviceProvider, "test-provider"));
            Assert.Contains("AzureTableStorageOptions", ex.Message);
        }
    }
}
