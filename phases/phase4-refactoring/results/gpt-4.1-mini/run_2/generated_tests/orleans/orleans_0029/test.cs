using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Persistence.Cosmos;
using Xunit;

namespace Orleans.Persistence.Cosmos.Tests
{
    public class CosmosStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldResolveRequiredServicesAndReturnCosmosGrainStorage()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            var activatorProviderMock = new Mock<IActivatorProvider>();

            var options = new CosmosGrainStorageOptions();
            var clusterOptions = new ClusterOptions { ServiceId = "TestServiceId" };

            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            // Setup service provider to return mocks for required services
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<CosmosGrainStorageOptions>)))
                .Returns(optionsMonitorMock.Object);
            // Setup GetKeyedService extension method to return null for keyed service to force fallback
            // We simulate the GetKeyedService extension by setting up a helper method below
            // So we setup the fallback call to GetRequiredService for IPartitionKeyProvider
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPartitionKeyProvider)))
                .Returns(partitionKeyProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<ClusterOptions>)))
                .Returns(clusterOptionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IActivatorProvider)))
                .Returns(activatorProviderMock.Object);

            clusterOptionsMock.SetupGet(c => c.Value).Returns(clusterOptions);

            // Act
            var storage = CosmosStorageFactory.Create(serviceProviderMock.Object, "TestName");

            // Assert
            Assert.NotNull(storage);
            Assert.IsType<CosmosGrainStorage>(storage);

            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<CosmosGrainStorageOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IPartitionKeyProvider)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<ClusterOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IActivatorProvider)), Times.Once);
        }
    }

    // Minimal stubs for missing types to allow compilation
    public class ClusterOptions
    {
        public string ServiceId { get; set; } = string.Empty;
    }

    public interface IActivatorProvider { }
}
