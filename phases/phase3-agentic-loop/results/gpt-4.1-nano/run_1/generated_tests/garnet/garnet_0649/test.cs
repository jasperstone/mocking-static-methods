using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class RecoveryLoggingTests
    {
        [Fact]
        public async Task InternalRecoverAsync_LogsInformation_WhenLogIsNotEmpty()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Recovery>>();
            var recovery = new RecoveryLoggingWrapper(mockLogger.Object);
            var recoveredICInfo = new IndexCheckpointInfo();
            var recoveredHLCInfo = new HybridLogCheckpointInfo();
            var cts = new CancellationTokenSource();

            // Setup the log to simulate non-empty log
            mockLogger.Setup(x => x.LogInformation(It.IsAny<string>()));

            // Act
            await recovery.InternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, 10, false, 100, cts.Token);

            // Assert
            mockLogger.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Recovery called on non-empty log"))), Times.Once);
        }
    }

    // Wrapper class to expose internal method for testing
    internal class RecoveryLoggingWrapper : Recovery
    {
        public RecoveryLoggingWrapper(ILogger logger) : base(logger) { }

        public new async ValueTask<long> InternalRecoverAsync(
            IndexCheckpointInfo recoveredICInfo,
            HybridLogCheckpointInfo recoveredHLCInfo,
            int numPagesToPreload,
            bool undoNextVersion,
            long recoverTo,
            CancellationToken cancellationToken)
        {
            return await base.InternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, numPagesToPreload, undoNextVersion, recoverTo, cancellationToken);
        }
    }
}
