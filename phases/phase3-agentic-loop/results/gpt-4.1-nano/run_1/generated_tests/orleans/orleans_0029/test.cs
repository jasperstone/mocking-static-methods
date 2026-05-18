using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Orleans.Persistence.Cosmos;
using Orleans;
using Orleans.Storage;
using Microsoft.Azure.Cosmos;

namespace Orleans.Tests
{
    public class CosmosGrainStorageTests
    {
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IOptions<ClusterOptions>> _clusterOptionsMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<CosmosGrainStorage>> _loggerMock;
        private readonly Mock<ICosmosOperationExecutor> _executorMock;
        private readonly Mock<Container> _containerMock;
        private readonly CosmosGrainStorageOptions _options;
        private readonly string _storageName = "testStorage";

        public CosmosGrainStorageTests()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<CosmosGrainStorage>>();
            _executorMock = new Mock<ICosmosOperationExecutor>();
            _containerMock = new Mock<Container>();
            _options = new CosmosGrainStorageOptions
            {
                OperationExecutor = _executorMock.Object,
                PartitionKeyPath = "/PartitionKey",
                ContainerName = "TestContainer"
            };

            _loggerFactoryMock.Setup(f => f.CreateLogger<CosmosGrainStorage>()).Returns(_loggerMock.Object);
            _clusterOptionsMock.Setup(c => c.Value).Returns(new ClusterOptions { ServiceId = "TestService" });
            _serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
                .Returns(new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>().Object);
        }

        [Fact]
        public void Create_CallsGetRequiredServiceForOptionsMonitor()
        {
            // Arrange
            var services = new ServiceCollection()
                .AddSingleton(_serviceProviderMock.Object)
                .BuildServiceProvider();

            // Act
            var storage = CosmosStorageFactory.Create(services, _storageName);

            // Assert
            _serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>(), Times.Once);
        }

        [Fact]
        public async Task ReadStateAsync_CallsContainerReadItemAsync_WhenEntityExists()
        {
            // Arrange
            var grainType = "TestGrain";
            var grainId = GrainId.NewGrainId();
            var grainState = new Mock<IGrainState<object>>();
            var options = _options;
            var storage = new CosmosGrainStorage(_storageName, options, _loggerFactoryMock.Object, _serviceProviderMock.Object, _clusterOptionsMock.Object, null, null);
            var resourceResponse = new Mock<ItemResponse<GrainStateEntity<object>>>();
            resourceResponse.Setup(r => r.Resource).Returns(new GrainStateEntity<object> { State = new object(), ETag = "etag" });
            _executorMock.Setup(e => e.ExecuteOperation(It.IsAny<Func<object, string, PartitionKey, Task<ItemResponse<GrainStateEntity<object>>>>>(), It.IsAny<(object, string, PartitionKey)>())).ReturnsAsync(resourceResponse.Object);

            // Act
            await storage.ReadStateAsync(grainType, grainId, grainState.Object);

            // Assert
            _executorMock.Verify(e => e.ExecuteOperation(It.IsAny<Func<object, string, PartitionKey, Task<ItemResponse<GrainStateEntity<object>>>>>(), It.IsAny<(object, string, PartitionKey)>()), Times.Once);
        }

        [Fact]
        public async Task WriteStateAsync_CallsContainerCreateItemAsync_WhenEtagIsNullOrEmpty()
        {
            // Arrange
            var grainType = "TestGrain";
            var grainId = GrainId.NewGrainId();
            var grainState = new Mock<IGrainState<object>>();
            var options = _options;
            var storage = new CosmosGrainStorage(_storageName, options, _loggerFactoryMock.Object, _serviceProviderMock.Object, _clusterOptionsMock.Object, null, null);
            var response = new Mock<ItemResponse<GrainStateEntity<object>>>();
            response.Setup(r => r.Resource).Returns(new GrainStateEntity<object> { ETag = "etag" });
            _executorMock.Setup(e => e.ExecuteOperation(It.IsAny<Func<object, GrainStateEntity<object>, PartitionKey, Task<ItemResponse<GrainStateEntity<object>>>>>(), It.IsAny<(object, GrainStateEntity<object>, PartitionKey)>())).ReturnsAsync(response.Object);

            // Act
            await storage.WriteStateAsync(grainType, grainId, grainState.Object);

            // Assert
            _executorMock.Verify(e => e.ExecuteOperation(It.IsAny<Func<object, GrainStateEntity<object>, PartitionKey, Task<ItemResponse<GrainStateEntity<object>>>>>(), It.IsAny<(object, GrainStateEntity<object>, PartitionKey)>()), Times.Once);
        }

        [Fact]
        public async Task ClearStateAsync_CallsContainerReadItemAsync_WhenDeleteStateOnClearAndETagIsEmpty()
        {
            // Arrange
            var grainType = "TestGrain";
            var grainId = GrainId.NewGrainId();
            var grainState = new Mock<IGrainState<object>>();
            var options = _options;
            var storage = new CosmosGrainStorage(_storageName, options, _loggerFactoryMock.Object, _serviceProviderMock.Object, _clusterOptionsMock.Object, null, null);
            var response = new Mock<ItemResponse<GrainStateEntity<object>>>();
            response.Setup(r => r.Resource).Returns(new GrainStateEntity<object> { ETag = "etag" });
            _executorMock.Setup(e => e.ExecuteOperation(It.IsAny<Func<object, string, PartitionKey, Task<ItemResponse<GrainStateEntity<object>>>>>(), It.IsAny<(object, string, PartitionKey)>())).ReturnsAsync(response.Object);

            // Act
            await storage.ClearStateAsync(grainType, grainId, grainState.Object);

            // Assert
            _executorMock.Verify(e => e.ExecuteOperation(It.IsAny<Func<object, string, PartitionKey, Task<ItemResponse<GrainStateEntity<object>>>>>(), It.IsAny<(object, string, PartitionKey)>()), Times.Once);
        }
    }
}
