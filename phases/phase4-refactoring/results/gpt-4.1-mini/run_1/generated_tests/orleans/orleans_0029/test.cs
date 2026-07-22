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
        public class ClusterOptions
        {
            public string ServiceId { get; set; } = string.Empty;
        }

        public interface IActivatorProvider { }

        [Fact]
        public void Create_ShouldResolveRequiredServicesAndReturnCosmosGrainStorage_WithFallbackPartitionKeyProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>(MockBehavior.Strict);
            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>(MockBehavior.Strict);
            var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>(MockBehavior.Strict);
            var activatorProviderMock = new Mock<IActivatorProvider>(MockBehavior.Strict);

            var providerName = "TestProvider";
            var cosmosOptions = new CosmosGrainStorageOptions();

            // Setup service provider to return mocks for GetRequiredService calls
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<CosmosGrainStorageOptions>)))
                .Returns(optionsMonitorMock.Object);
            // Simulate GetKeyedService returns null, so fallback to GetRequiredService
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPartitionKeyProvider)))
                .Returns(partitionKeyProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<ClusterOptions>)))
                .Returns(clusterOptionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IActivatorProvider)))
                .Returns(activatorProviderMock.Object);

            // Setup options monitor to return cosmosOptions for the given provider name
            optionsMonitorMock.Setup(m => m.Get(providerName)).Returns(cosmosOptions);

            // Setup cluster options value
            var clusterOptions = new ClusterOptions { ServiceId = "TestServiceId" };
            clusterOptionsMock.SetupGet(c => c.Value).Returns(clusterOptions);

            // Act
            var storage = CosmosStorageFactory.Create(serviceProviderMock.Object, providerName);

            // Assert
            Assert.NotNull(storage);
            Assert.IsType<CosmosGrainStorage>(storage);

            // Verify all expected calls
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<CosmosGrainStorageOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IPartitionKeyProvider)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<ClusterOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IActivatorProvider)), Times.Once);
            optionsMonitorMock.Verify(m => m.Get(providerName), Times.Once);
            clusterOptionsMock.VerifyGet(c => c.Value, Times.Once);
        }
    }
}
