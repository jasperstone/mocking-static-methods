using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.AzureStorage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        private class TestServiceProvider : IServiceProvider
        {
            private readonly IOptionsMonitor<AzureTableStorageOptions> optionsMonitor;
            private readonly IOptions<ClusterOptions> clusterOptions;

            public TestServiceProvider(IOptionsMonitor<AzureTableStorageOptions> optionsMonitor, IOptions<ClusterOptions> clusterOptions)
            {
                this.optionsMonitor = optionsMonitor;
                this.clusterOptions = clusterOptions;
            }

            public object? GetService(Type serviceType)
            {
                if (serviceType == typeof(IOptionsMonitor<AzureTableStorageOptions>))
                    return optionsMonitor;
                if (serviceType == typeof(IOptions<ClusterOptions>))
                    return clusterOptions;
                return null;
            }
        }

        [Fact]
        public void Create_ShouldCallGetRequiredServiceAndCreateInstance()
        {
            // Arrange
            var options = new AzureTableStorageOptions();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            var clusterOptions = new ClusterOptions();

            var serviceProvider = new TestServiceProvider(optionsMonitorMock.Object, Options.Create(clusterOptions));

            // Act
            var storage = AzureTableGrainStorageFactory.Create(serviceProvider, "TestName");

            // Assert
            Assert.NotNull(storage);
            optionsMonitorMock.Verify(m => m.Get("TestName"), Times.Once);
        }
    }
}
