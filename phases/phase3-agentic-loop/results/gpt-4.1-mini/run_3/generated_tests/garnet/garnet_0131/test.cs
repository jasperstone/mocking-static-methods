using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class MigrateSessionTests
    {
        // Helper to create a MigrateSession with mocked dependencies and logger
        private MigrateSession CreateMigrateSessionWithLogger(out Mock<ILogger> loggerMock, out Mock<ILoggerFactory> loggerFactoryMock)
        {
            loggerMock = new Mock<ILogger>();
            loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.SetupGet(p => p.loggerFactory).Returns(loggerFactoryMock.Object);

            // Setup minimal required constructor parameters
            var slots = new HashSet<int> { 1, 2, 3 };
            var clusterSession = null as ClusterSession;
            var sketch = null as Sketch;

            // Use TransferOption.SLOTS to trigger BeginAsyncMigrationTaskAsync path
            var migrateSession = new MigrateSession(
                clusterSession,
                clusterProviderMock.Object,
                "127.0.0.1",
                6379,
                "targetNodeId",
                "user",
                "pass",
                "sourceNodeId",
                false,
                false,
                1000,
                slots,
                sketch,
                TransferOption.SLOTS);

            return migrateSession;
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTrySetSlotRangesAsyncFails()
        {
            // Arrange
            var migrateSession = CreateMigrateSessionWithLogger(out var loggerMock, out var loggerFactoryMock);

            // Setup TrySetSlotRangesAsync to return false to trigger LogError call on line 154
            var migrateSessionPrivate = migrateSession as dynamic;
            migrateSessionPrivate.TrySetSlotRangesAsync = new Func<string, MigrateState, Task<bool>>((nodeId, state) => Task.FromResult(false));
            migrateSessionPrivate.GetSourceNodeId = "sourceNodeId";
            migrateSessionPrivate.GetSlots = new HashSet<int> { 1, 2, 3 };

            // Setup TryRecoverFromFailureAsync to return completed task
            migrateSessionPrivate.TryRecoverFromFailureAsync = new Func<Task<bool>>(() => Task.FromResult(true));

            // Act
            // Call the private method BeginAsyncMigrationTaskAsync via reflection
            var method = typeof(MigrateSession).GetMethod("BeginAsyncMigrationTaskAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task)method.Invoke(migrateSession, null);
            await task;

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set remote slots")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenTryPrepareLocalForMigrationFails()
        {
            // Arrange
            var migrateSession = CreateMigrateSessionWithLogger(out var loggerMock, out var loggerFactoryMock);

            var migrateSessionPrivate = migrateSession as dynamic;

            // Setup TrySetSlotRangesAsync to return true to pass first check
            migrateSessionPrivate.TrySetSlotRangesAsync = new Func<string, MigrateState, Task<bool>>((nodeId, state) => Task.FromResult(true));
            migrateSessionPrivate.GetSourceNodeId = "sourceNodeId";
            migrateSessionPrivate.GetSlots = new HashSet<int> { 1, 2, 3 };

            // Setup TryPrepareLocalForMigration to return false to trigger LogError on line 154
            migrateSessionPrivate.TryPrepareLocalForMigration = new Func<bool>(() => false);

            // Setup TryRecoverFromFailureAsync to return completed task
            migrateSessionPrivate.TryRecoverFromFailureAsync = new Func<Task<bool>>(() => Task.FromResult(true));

            // Setup clusterProvider.BumpAndWaitForEpochTransitionAsync to return true
            migrateSessionPrivate.clusterProvider = new Mock<ClusterProvider>().Object;
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.Setup(p => p.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);
            migrateSessionPrivate.clusterProvider = clusterProviderMock.Object;

            // Act
            var method = typeof(MigrateSession).GetMethod("BeginAsyncMigrationTaskAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task)method.Invoke(migrateSession, null);
            await task;

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set local slots")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
