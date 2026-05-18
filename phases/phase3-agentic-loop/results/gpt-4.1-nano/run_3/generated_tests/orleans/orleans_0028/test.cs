using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Storage;
using Orleans.Providers.Azure;
using Orleans.Runtime;

namespace Orleans.Tests
{
    public class AzureTableGrainStorageTests
    {
        private readonly Mock<IServiceProvider> serviceProviderMock;
        private readonly Mock<IOptions<ClusterOptions>> clusterOptionsMock;
        private readonly Mock<ILogger<AzureTableGrainStorage>> loggerMock;
        private readonly Mock<IActivatorProvider> activatorProviderMock;
        private readonly AzureTableStorageOptions options;
        private readonly string storageName = "TestStorage";

        public AzureTableGrainStorageTests()
        {
            serviceProviderMock = new Mock<IServiceProvider>();
            clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            loggerMock = new Mock<ILogger<AzureTableGrainStorage>>();
            activatorProviderMock = new Mock<IActivatorProvider>();
            clusterOptionsMock.Setup(c => c.Value).Returns(new ClusterOptions { ClusterId = "TestCluster" });
            options = new AzureTableStorageOptions
            {
                GrainStorageSerializer = new DefaultGrainStorageSerializer()
            };
        }

        [Fact]
        public void Create_ShouldResolveOptionsAndClusterOptions()
        {
            // Arrange
            var services = new ServiceCollection()
                .AddSingleton(clusterOptionsMock.Object)
                .BuildServiceProvider();

            // Act
            var storage = new AzureTableGrainStorage(storageName, options, services.GetRequiredService<IOptions<ClusterOptions>>(), loggerMock.Object, activatorProviderMock.Object);

            // Assert
            Assert.NotNull(storage);
        }

        [Fact]
        public async Task ReadStateAsync_ShouldCallTableDataManagerRead_WhenInitialized()
        {
            // Arrange
            var storage = new AzureTableGrainStorage(storageName, options, clusterOptionsMock.Object, loggerMock.Object, activatorProviderMock.Object);
            var mockTableDataManager = new Mock<GrainStateTableDataManager>();
            storage.GetType().GetField("tableDataManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(storage, mockTableDataManager.Object);

            var grainState = new Mock<IGrainState<object>>();
            var grainId = GrainId.NewGrainId(0, "test");
            var grainType = "TestGrain";

            mockTableDataManager.Setup(m => m.Read(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((TableEntity?)null);

            // Act
            await storage.ReadStateAsync(grainType, grainId, grainState.Object);

            // Assert
            mockTableDataManager.Verify(m => m.Read(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task WriteStateAsync_ShouldCallTableDataManagerWrite_WhenInitialized()
        {
            // Arrange
            var storage = new AzureTableGrainStorage(storageName, options, clusterOptionsMock.Object, loggerMock.Object, activatorProviderMock.Object);
            var mockTableDataManager = new Mock<GrainStateTableDataManager>();
            storage.GetType().GetField("tableDataManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(storage, mockTableDataManager.Object);

            var grainState = new Mock<IGrainState<object>>();
            var grainId = GrainId.NewGrainId(0, "test");
            var grainType = "TestGrain";

            mockTableDataManager.Setup(m => m.Write(It.IsAny<TableEntity>()))
                .Returns(Task.CompletedTask);

            // Act
            await storage.WriteStateAsync(grainType, grainId, grainState.Object);

            // Assert
            mockTableDataManager.Verify(m => m.Write(It.IsAny<TableEntity>()), Times.Once);
        }

        [Fact]
        public async Task ClearStateAsync_ShouldCallTableDataManagerDelete_WhenDeleteStateOnClearIsTrue()
        {
            // Arrange
            var optionsWithDelete = new AzureTableStorageOptions { DeleteStateOnClear = true };
            var storage = new AzureTableGrainStorage(storageName, optionsWithDelete, clusterOptionsMock.Object, loggerMock.Object, activatorProviderMock.Object);
            var mockTableDataManager = new Mock<GrainStateTableDataManager>();
            storage.GetType().GetField("tableDataManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(storage, mockTableDataManager.Object);

            var grainState = new Mock<IGrainState<object>>();
            var grainId = GrainId.NewGrainId(0, "test");
            var grainType = "TestGrain";

            mockTableDataManager.Setup(m => m.Delete(It.IsAny<TableEntity>()))
                .Returns(Task.CompletedTask);

            // Act
            await storage.ClearStateAsync(grainType, grainId, grainState.Object);

            // Assert
            mockTableDataManager.Verify(m => m.Delete(It.IsAny<TableEntity>()), Times.Once);
        }

        [Fact]
        public async Task ClearStateAsync_ShouldCallTableDataManagerWrite_WhenDeleteStateOnClearIsFalse()
        {
            // Arrange
            var optionsWithWrite = new AzureTableStorageOptions { DeleteStateOnClear = false };
            var storage = new AzureTableGrainStorage(storageName, optionsWithWrite, clusterOptionsMock.Object, loggerMock.Object, activatorProviderMock.Object);
            var mockTableDataManager = new Mock<GrainStateTableDataManager>();
            storage.GetType().GetField("tableDataManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(storage, mockTableDataManager.Object);

            var grainState = new Mock<IGrainState<object>>();
            var grainId = GrainId.NewGrainId(0, "test");
            var grainType = "TestGrain";

            mockTableDataManager.Setup(m => m.Write(It.IsAny<TableEntity>()))
                .Returns(Task.CompletedTask);

            // Act
            await storage.ClearStateAsync(grainType, grainId, grainState.Object);

            // Assert
            mockTableDataManager.Verify(m => m.Write(It.IsAny<TableEntity>()), Times.Once);
        }
    }
}
