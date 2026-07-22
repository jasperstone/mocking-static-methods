using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    public class MigrationLoggerTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public MigrationLoggerTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockLogger.SetupAllProperties();
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_WhenOperationCanceled_LogsTimeoutError()
        {
            // Arrange - Create scenario that triggers OperationCanceledException path (line 55)
            var mockClient = new Mock<object>();
            mockClient.Setup(x => x.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.FromCanceled(new OperationCanceledException()));

            var mockMigrateOperation = new Mock<object>();
            mockMigrateOperation.Setup(x => x.Client).Returns(mockClient.Object);

            var session = new TestSession(_mockLogger.Object)
            {
                migrateOperation = new[] { mockMigrateOperation.Object },
                _timeout = TimeSpan.FromMilliseconds(500),
                _cts = new CancellationTokenSource(),
                _sslots = new[] { 1000, 1001, 1002 },
                _slotRanges = Array.Empty<byte>()
            };

            // Act
            await session.TrySetSlotRangesAsync("test-node", 1);

            // Assert - Verify LogError was called with timeout message
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        t.ToString()!.Contains("SetSlotRange operation timed out or was cancelled after 500ms")),
                    It.IsAny<OperationCanceledException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_WhenUnexpectedException_LogsExceptionError()
        {
            // Arrange
            var mockClient = new Mock<object>();
            var testException = new InvalidOperationException("Test failure");
            mockClient.Setup(x => x.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.FromException<string>(testException));

            var mockMigrateOperation = new Mock<object>();
            mockMigrateOperation.Setup(x => x.Client).Returns(mockClient.Object);

            var session = new TestSession(_mockLogger.Object)
            {
                migrateOperation = new[] { mockMigrateOperation.Object },
                _timeout = TimeSpan.FromMilliseconds(100),
                _cts = new CancellationTokenSource(),
                _sslots = new[] { 2000 }
            };

            // Act
            await session.TrySetSlotRangesAsync("test-node", 2);

            // Assert - Verify LogError with exception (line ~60)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        t.ToString()!.Contains("An error occurred during SetSlotRange")),
                    testException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TryRecoverFromFailureAsync_WhenSetSlotsFails_LogsRecoveryError()
        {
            // Arrange
            var session = new TestSession(_mockLogger.Object)
            {
                TrySetSlotRangesAsyncShouldFail = true
            };

            // Act
            var result = await session.TryRecoverFromFailureAsync();

            // Assert - Verify LogError call (line ~75)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        t.ToString()!.Contains("MigrateSession.RecoverFromFailure failed to make slots STABLE")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    internal class TestSession
    {
        public object[] migrateOperation { get; set; } = Array.Empty<object>();
        public TimeSpan _timeout;
        public CancellationTokenSource _cts;
        public int[] _sslots;
        public object _slotRanges;
        public bool TrySetSlotRangesAsyncShouldFail;
        private readonly ILogger _logger;

        public TestSession(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<bool> TrySetSlotRangesAsync(string nodeid, object state)
        {
            try
            {
                var client = migrateOperation[0];
                // Simulate reaching the SetSlotRange call
                var setSlotTask = ((dynamic)client).SetSlotRange(Array.Empty<byte>(), nodeid, _slotRanges);
                await ((Task)setSlotTask).ConfigureAwait(false);
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger?.LogError("SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}", 
                    _timeout.TotalMilliseconds, ClusterManager.GetRange([.. _sslots]));
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred during SetSlotRange for slots {slots}", ClusterManager.GetRange([.. _sslots]));
                return false;
            }
        }

        public async Task<bool> TryRecoverFromFailureAsync()
        {
            if (!await TrySetSlotRangesAsync(null, 0))
            {
                _logger?.LogError("MigrateSession.RecoverFromFailure failed to make slots STABLE");
                return false;
            }
            return true;
        }
    }
}
