using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Storage;
using Orleans.Configuration;
using Xunit;

namespace Orleans.Persistence.AzureStorage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldCallGetRequiredServiceAndReturnInstance()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var clusterOptions = new ClusterOptions { ClusterId = "test-cluster" };

            var options = new AzureTableStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            // Setup GetService to return the mocked IOptionsMonitor
            servicesMock.Setup(s => s.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // We cannot mock extension methods directly, so we create a wrapper IServiceProvider that returns clusterOptions for GetProviderClusterOptions
            var serviceProvider = new TestServiceProvider(servicesMock.Object, clusterOptions);

            // Act
            var storage = AzureTableGrainStorageFactory.Create(serviceProvider, "test-name");

            // Assert
            Assert.NotNull(storage);
            optionsMonitorMock.Verify(m => m.Get("test-name"), Times.Once);
            servicesMock.Verify(s => s.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)), Times.Once);
        }

        private class TestServiceProvider : IServiceProvider
        {
            private readonly IServiceProvider inner;
            private readonly ClusterOptions clusterOptions;

            public TestServiceProvider(IServiceProvider inner, ClusterOptions clusterOptions)
            {
                this.inner = inner;
                this.clusterOptions = clusterOptions;
            }

            public object? GetService(Type serviceType)
            {
                if (serviceType == typeof(ClusterOptions))
                {
                    return clusterOptions;
                }
                return inner.GetService(serviceType);
            }
        }
    }
}
