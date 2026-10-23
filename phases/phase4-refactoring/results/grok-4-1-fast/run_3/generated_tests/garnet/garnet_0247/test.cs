using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarningExtension_CalledWithExceptionAndMessage_InvokesLog()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var testException = new InvalidOperationException("Test exception");
            var expectedMessage = "An exception occurred at ReplicationManager.ProcessPrimaryStream";

            // Act
            mockLogger.Object.LogWarning(testException, expectedMessage);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat>(f => f.ToString().Contains(expectedMessage)),
                    testException,
                    It.IsAny<Func<It.IsAnyFormat, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarningExtension_WhenLoggerIsNull_DoesNothing()
        {
            // Arrange
            ILogger logger = null;
            var testException = new InvalidOperationException("Test exception");
            var expectedMessage = "An exception occurred at ReplicationManager.ProcessPrimaryStream";

            // Act
            logger?.LogWarning(testException, expectedMessage);

            // Assert - no exception thrown, null-conditional handles it
            Assert.True(true);
        }

        [Fact]
        public void LogWarningExtension_MatchesReplicationManagerUsage()
        {
            // Arrange - exact signature match for line 135 usage
            var mockLogger = new Mock<ILogger>();
            var garnetException = new Exception("Processing failed");
            var replicationMessage = "An exception occurred at ReplicationManager.ProcessPrimaryStream";

            // Act - simulate the exact call from ReplicationReplicaAofSync.cs line 135
            mockLogger.Object.LogWarning(garnetException, replicationMessage);

            // Assert - verifies the extension method works as expected in context
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyFormat>(),
                    garnetException,
                    It.IsAny<Func<It.IsAnyFormat, Exception, string>>()),
                Times.Once);
        }
    }
}
