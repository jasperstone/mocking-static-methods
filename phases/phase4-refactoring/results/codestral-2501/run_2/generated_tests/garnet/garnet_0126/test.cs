using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Garnet.cluster.Tests
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_ShouldLogErrorOnTimeout()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var migrateSession = new MigrateSession(mockLogger.Object);

            // Act
            var result = await migrateSession.TrySetSlotRangesAsync("nodeid", MigrateState.IMPORT);

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
            Assert.False(result);
        }
    }

    internal class MigrateSession
    {
        private readonly ILogger logger;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(1);
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly string[] _sslots = new string[] { "slot1", "slot2" };

        public MigrateSession(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
        {
            try
            {
                // Simulate a timeout
                await Task.Delay(_timeout.Add(TimeSpan.FromSeconds(1)), _cts.Token);
            }
            catch (OperationCanceledException)
            {
                logger?.LogError("SetSlotRange operation timed out or was cancelled after {timeout}ms for slots {slots}", _timeout.TotalMilliseconds, ClusterManager.GetRange([.. _sslots]));
                return false;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurred during SetSlotRange for slots {slots}", ClusterManager.GetRange([.. _sslots]));
                return false;
            }

            return true;
        }
    }

    internal static class ClusterManager
    {
        public static string GetRange(string[] slots)
        {
            return string.Join(",", slots);
        }
    }

    internal enum MigrateState
    {
        IMPORT,
        STABLE,
        NODE,
        FAIL,
        SUCCESS
    }
}
