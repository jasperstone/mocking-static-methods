using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using System.Threading;
using System.Threading.Tasks;

namespace Tsavorite.Tests
{
    public class RecoveryTests
    {
        [Fact]
        public async Task InternalRecoverAsync_LogsInformation_WhenTailAddressGreaterThanFirstValidLogicalAddress()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Recovery>>();
            var recovery = new Recovery(mockLogger.Object);

            var recoveredICInfo = new IndexCheckpointInfo();
            var recoveredHLCInfo = new HybridLogCheckpointInfo();
            var numPagesToPreload = 10;
            var undoNextVersion = false;
            var recoverTo = 100L;
            var cancellationToken = CancellationToken.None;

            // Act
            await recovery.InternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, numPagesToPreload, undoNextVersion, recoverTo, cancellationToken);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
