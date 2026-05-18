using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_Should_LogError_When_TrySetSlotRangesAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var clusterManagerMock = new Mock<IClusterManager>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var migrationManagerMock = new Mock<IMigrationManager>();
            var clientMock = new Mock<IRedisClient>();

            // Setup the MigrateSession with minimal dependencies
            var migrateSession = new MigrateSession(
                loggerMock.Object,
                clusterProviderMock.Object,
                clusterManagerMock.Object,
                storeWrapperMock.Object,
                migrationManagerMock.Object,
                clientMock.Object);

            // Setup dependencies to simulate failure in TrySetSlotRangesAsync
            // For simplicity, we will override the method via a derived class
            var testSession = new TestMigrateSession(
                loggerMock.Object,
                clusterProviderMock.Object,
                clusterManagerMock.Object,
                storeWrapperMock.Object,
                migrationManagerMock.Object,
                clientMock.Object);

            // Act
            await testSession.InvokeBeginAsyncMigrationTaskAsync();

            // Assert
            // Verify that LogError was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set remote slots")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Derived class to override the private method for testing
        private class TestMigrateSession : MigrateSession
        {
            public TestMigrateSession(
                ILogger<MigrateSession> logger,
                IClusterProvider clusterProvider,
                IClusterManager clusterManager,
                IStoreWrapper storeWrapper,
                IMigrationManager migrationManager,
                IRedisClient client)
                : base(logger, clusterProvider, clusterManager, storeWrapper, migrationManager, client)
            {
            }

            public async Task InvokeBeginAsyncMigrationTaskAsync()
            {
                await BeginAsyncMigrationTaskAsync();
            }

            protected override async Task<bool> TrySetSlotRangesAsync(string nodeId, MigrateState state)
            {
                // Simulate failure to trigger LogError
                return false;
            }
        }
    }
}
