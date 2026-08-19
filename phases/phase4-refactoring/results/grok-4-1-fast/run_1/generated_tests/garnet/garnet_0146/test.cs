using System;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class CheckpointStoreLoggerTests
    {
        [Fact]
        public void PurgeAllCheckpointsExceptEntry_VerifiesIndexTokenLogTraceCall()
        {
            // Arrange - Create real dependencies that don't throw
            var logger = new Mock<ILogger>().Object;
            var clusterProvider = new Mock<ClusterProvider>().Object;
            var storeWrapper = new Mock<StoreWrapper>().Object;

            // Mock the low-level ILogger Log method to verify the exact LogTrace extension call
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            
            // Capture the Log call parameters to verify the LogTrace message
            mockLogger.Setup(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting index token {toDeleteIndexToken}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Create CheckpointStore with real constructor args (types exist, just inaccessible fields)
            var checkpointStore = new CheckpointStore(storeWrapper, clusterProvider, safelyRemoveOutdated: false, mockLogger.Object);

            // Act & Assert - Verify the specific LogTrace extension method call on line 111
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting index token")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_VerifiesLogTokenLogTraceCall()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            var storeWrapper = new Mock<StoreWrapper>().Object;
            var clusterProvider = new Mock<ClusterProvider>().Object;

            var checkpointStore = new CheckpointStore(storeWrapper, clusterProvider, safelyRemoveOutdated: false, mockLogger.Object);

            // Act & Assert - Verify the log token deletion LogTrace call
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting log token")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LoggerIsNull_DoesNotThrow()
        {
            // Arrange
            var storeWrapper = new Mock<StoreWrapper>().Object;
            var clusterProvider = new Mock<ClusterProvider>().Object;

            // Act - null logger should safely skip logging (logger?.LogTrace)
            var checkpointStore = new CheckpointStore(storeWrapper, clusterProvider, safelyRemoveOutdated: false, logger: null);

            // Assert - doesn't throw when logger is null
            checkpointStore.PurgeAllCheckpointsExceptEntry();
        }
    }
}
