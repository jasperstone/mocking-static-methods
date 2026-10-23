using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;

namespace Garnet.cluster.Server.Replication.PrimaryOps.DisklessReplication.Tests
{
    public class ReplicaSyncSessionLoggerTests
    {
        private readonly Mock<ILogger<ReplicaSyncSession>> _mockLogger;

        public ReplicaSyncSessionLoggerTests()
        {
            _mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
        }

        [Fact]
        public async Task WaitForFlushAsync_WhenFlushTaskThrowsException_LogsError()
        {
            // Arrange
            var session = CreateSession();
            var exception = new InvalidOperationException("Flush failed");
            var faultedTask = Task.FromException<bool>(exception);
            
            SetPrivateField(session, "logger", _mockLogger.Object);
            SetPrivateField(session, "flushTask", faultedTask);

            // Act
            await session.WaitForFlushAsync();

            // Assert - Verifies coverage of LogError call at line ~203 equivalent
            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "{method}",
                    It.Is<string>(s => s == "WaitForFlushAsync")),
                Times.Once);
        }

        [Fact]
        public async Task WaitForSyncCompletionAsync_WhenSignalWaitThrowsException_LogsError()
        {
            // Arrange
            var session = CreateSession();
            var exception = new OperationCanceledException("Sync timeout");
            var mockSignal = new Mock<AsyncManualResetEvent>();
            mockSignal.Setup(x => x.WaitAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);
            
            var cts = new CancellationTokenSource();
            
            SetPrivateField(session, "logger", _mockLogger.Object);
            SetPrivateField(session, "signalCompletion", mockSignal.Object);
            SetPrivateField(session, "token", cts.Token);

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => session.WaitForSyncCompletionAsync());

            // Assert - Verifies coverage of LogError call specifically targeted (line 203)
            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "{method} failed waiting for sync",
                    It.Is<string>(s => s == "WaitForSyncCompletionAsync")),
                Times.Once);
        }

        private ReplicaSyncSession CreateSession()
        {
            // Use reflection to create private/internal instance
            return (ReplicaSyncSession)Activator.CreateInstance(
                typeof(ReplicaSyncSession), 
                BindingFlags.NonPublic | BindingFlags.Instance, 
                null, 
                null, 
                null)!;
        }

        private void SetPrivateField(object target, string fieldName, object value)
        {
            var field = typeof(ReplicaSyncSession).GetField(fieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            field.SetValue(target, value);
        }
    }
}
