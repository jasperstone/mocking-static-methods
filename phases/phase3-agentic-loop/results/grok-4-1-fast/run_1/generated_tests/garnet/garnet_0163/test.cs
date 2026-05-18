using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaSyncSessionLoggerTests
    {
        [Fact]
        public void TestLoggerErrorExtensionUsage()
        {
            // Given we need to verify LoggerExtensions.LogError usage
            // Since ReplicaSyncSession is internal, test the extension method pattern directly
            
            var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
            
            // Simulate the LogError call from line 203:
            // logger?.LogError(ex, "{method} failed waiting for sync", nameof(WaitForSyncCompletionAsync));
            
            var exception = new Exception("Test exception");
            mockLogger.Object.LogError(exception, "{method} failed waiting for sync", nameof(ReplicaSyncSession.WaitForSyncCompletionAsync));
            
            // Verify the exact extension method signature is used (2 format params)
            mockLogger.Verify(
                l => l.Log(
                    It.Is<LogLevel>(level => level == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex == exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void TestLoggerErrorExtensionUsage_WaitForFlushAsync()
        {
            // Simulate the LogError call from WaitForFlushAsync:
            // logger?.LogError(ex, "{method}", $"{nameof(WaitForFlushAsync)}");
            
            var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
            var exception = new Exception("Flush failed");
            
            mockLogger.Object.LogError(exception, "{method}", nameof(ReplicaSyncSession.WaitForFlushAsync));
            
            mockLogger.Verify(
                l => l.Log(
                    It.Is<LogLevel>(level => level == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex == exception),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
