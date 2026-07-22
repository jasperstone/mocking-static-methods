using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace MigrationDriverTests
{
    public class MigrationDriverTest
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_LogsTraceOnLine50()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var clientMock = new Mock<IRedisClient>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var vectorManagerMock = new Mock<IVectorManager>();
            var migrationManagerMock = new Mock<IMigrationManager>();

            // Create a minimal subclass to expose the method
            var session = new TestMigrateSession(loggerMock.Object, clientMock.Object);

            // Setup the client mock to return "OK" for SetSlotRange
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte>(), It.IsAny<string>(), It.IsAny<int[]>()))
                .ReturnsAsync("OK");

            // Act
            await session.TrySetSlotRangesAsync("node1", MigrateState.STABLE);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sending CLUSTER SETSLOTRANGE")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal subclass to expose the method for testing
    public class TestMigrateSession : MigrateSession
    {
        public TestMigrateSession(ILogger<MigrateSession> logger, IRedisClient client)
        {
            this.logger = logger;
            // Inject dependencies as needed
        }

        public new async Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
        {
            return await base.TrySetSlotRangesAsync(nodeid, state);
        }
    }
}
