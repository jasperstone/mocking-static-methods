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
        public void Create_ShouldCallGetRequiredServiceOnIServiceProvider_AndReturnCosmosGrainStorage()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            var services = serviceProviderMock.Object;
            var name = "TestStorage";

            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>(MockBehavior.Strict);
            var options = new CosmosGrainStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(name)).Returns(options);

            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>(MockBehavior.Strict);
            var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger>(MockBehavior.Strict);
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>(MockBehavior.Strict);
            var clusterOptions = new ClusterOptions { ServiceId = "TestServiceId" };
            var activatorProviderMock = new Mock<IActivatorProvider>(MockBehavior.Strict);

            // Setup GetRequiredService calls
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<CosmosGrainStorageOptions>)))
                .Returns(optionsMonitorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPartitionKeyProvider)))
                .Returns(partitionKeyProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<ClusterOptions>)))
                .Returns(clusterOptionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IActivatorProvider)))
                .Returns(activatorProviderMock.Object);

            // Setup GetKeyedService extension method behavior
            // Since GetKeyedService is an extension method, we simulate it by setting up a helper method
            // For this test, we simulate that GetKeyedService returns null, so fallback to GetRequiredService is used
            // We simulate this by setting up a helper extension method on the mock (not possible directly)
            // So we will mock the extension method by creating a helper class with the same signature and use it in the test
            // But since we cannot mock extension methods directly, we will simulate by creating a derived IServiceProvider that returns null for GetKeyedService

            // Setup logger factory to create logger
            loggerFactoryMock.Setup(lf => lf.CreateLogger<CosmosGrainStorage>()).Returns(loggerMock.Object);

            // Setup cluster options value
            clusterOptionsMock.SetupGet(co => co.Value).Returns(clusterOptions);

            // Act
            var storage = CosmosStorageFactory.Create(serviceProviderMock.Object, name);

            // Assert
            Assert.NotNull(storage);
            Assert.IsType<CosmosGrainStorage>(storage);

            // Verify that GetService was called for required services
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<CosmosGrainStorageOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IPartitionKeyProvider)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<ClusterOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IActivatorProvider)), Times.Once);

            loggerFactoryMock.Verify(lf => lf.CreateLogger<CosmosGrainStorage>(), Times.Once);
            optionsMonitorMock.Verify(m => m.Get(name), Times.Once);
            clusterOptionsMock.VerifyGet(co => co.Value, Times.Once);
        }
    }
}
