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
        public void Create_ShouldCallGetRequiredServiceOnIServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>(MockBehavior.Strict);
            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>(MockBehavior.Strict);
            var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>(MockBehavior.Strict);
            var activatorProviderMock = new Mock<IActivatorProvider>(MockBehavior.Strict);

            var name = "TestStorage";

            // Setup GetRequiredService calls
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<CosmosGrainStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IPartitionKeyProvider)))
                .Returns(partitionKeyProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptions<ClusterOptions>)))
                .Returns(clusterOptionsMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IActivatorProvider)))
                .Returns(activatorProviderMock.Object);

            // Setup GetKeyedService extension method behavior
            // Since GetKeyedService is an extension method, we simulate it by returning null to force fallback to GetRequiredService
            // We simulate this by creating an extension method in test or by mocking the call indirectly.
            // Here, we will mock the extension method by creating a helper class in test.

            // Setup optionsMonitor.Get(name) to return a dummy CosmosGrainStorageOptions
            var options = new CosmosGrainStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(name)).Returns(options);

            // Setup clusterOptions.Value
            var clusterOptions = new ClusterOptions { ServiceId = "TestServiceId" };
            clusterOptionsMock.SetupGet(c => c.Value).Returns(clusterOptions);

            // Act
            var storage = CosmosStorageFactory.Create(serviceProviderMock.Object, name);

            // Assert
            Assert.NotNull(storage);
            Assert.IsType<CosmosGrainStorage>(storage);

            // Verify all GetService calls
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<CosmosGrainStorageOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IPartitionKeyProvider)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<ClusterOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IActivatorProvider)), Times.Once);
        }
    }
}
