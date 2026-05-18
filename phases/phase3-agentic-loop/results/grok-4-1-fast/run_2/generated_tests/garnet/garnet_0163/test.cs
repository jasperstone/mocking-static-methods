using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Garnet.cluster.Server.Replication.PrimaryOps.DisklessReplication.Tests
{
    public class ReplicaSyncSessionLoggerTests
    {
        [Fact]
        public void WaitForFlushAsync_WhenFlushTaskThrows_CallsLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicaSyncSession>>();
            loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            
            var session = new ReplicaSyncSession();
            
            // Set logger via reflection
            var loggerField = typeof(ReplicaSyncSession)
                .GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField?.SetValue(session, loggerMock.Object);
            
            // Set faulted flushTask via reflection to trigger catch block (line 203)
            var faultedTask = Task.FromException<bool>(new InvalidOperationException("Flush failed"));
            var flushTaskField = typeof(ReplicaSyncSession)
                .GetField("flushTask", BindingFlags.NonPublic | BindingFlags.Instance);
            flushTaskField?.SetValue(session, faultedTask);

            // Act - Call via reflection since methods are internal
            var waitMethod = typeof(ReplicaSyncSession).GetMethod("WaitForFlushAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var task = (Task)waitMethod!.Invoke(session, null)!;
            task.GetAwaiter().GetResult();

            // Assert - Verify LogError was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void WaitForSyncCompletionAsync_WhenSignalThrows_CallsLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReplicaSyncSession>>();
            loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            
            var session = new ReplicaSyncSession();
            var loggerField = typeof(ReplicaSyncSession)
                .GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField?.SetValue(session, loggerMock.Object);
            
            // Mock signalCompletion to throw
            var signalMock = new Mock<SemaphoreSlim>();
            signalMock.Setup(s => s.WaitAsync(It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new InvalidOperationException("Signal failed"));
            
            var signalField = typeof(ReplicaSyncSession)
                .GetField("signalCompletion", BindingFlags.NonPublic | BindingFlags.Instance);
            signalField?.SetValue(session, signalMock.Object);

            // Act - Call via reflection
            var waitMethod = typeof(ReplicaSyncSession).GetMethod("WaitForSyncCompletionAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var task = (Task)waitMethod!.Invoke(session, null)!;
            
            // Assert - Verify LogError was called (line 215)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
