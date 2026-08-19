using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace MigrationTests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_ShouldLogError_WhenResultIsNotOk()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var clientMock = new Mock<IRedisClient>();
            var migrationManagerMock = new Mock<IMigrationManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var vectorManagerMock = new Mock<IVectorManager>();
            var defaultDatabaseMock = new Mock<IDatabase>();
            var storeMock = new Mock<IStore>();
            var clusterProviderMock = new Mock<IClusterProvider>();

            // Setup the client to return a non-"OK" string
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte>(), It.IsAny<string>(), It.IsAny<int[]>()))
                .ReturnsAsync("ERROR");

            // Create a MigrateSession instance with mocks
            var session = new MigrateSession(/* constructor parameters with mocks */);
            // Since constructor details are not fully visible, assume we can set dependencies after creation
            // or that the constructor is accessible for test purposes.

            // For the purpose of this example, assume we can set the logger and client directly
            // (In real code, you might need to use reflection or a constructor that accepts dependencies)

            // Act
            var result = await session.TrySetSlotRangesAsync("node1", MigrateState.STABLE);

            // Assert
            Assert.False(result);
            // Verify that LogError was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange error: ERROR")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
