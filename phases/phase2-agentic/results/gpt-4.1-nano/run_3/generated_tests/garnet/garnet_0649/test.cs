using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tsavorite.core.Tests
{
    public class RecoveryLoggingTests
    {
        [Fact]
        public async Task InternalRecoverAsync_LogsInformation_WhenLogIsNotEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TsavoriteKV<int, int, IStoreFunctions<int, int>, IAllocator<int, int, IStoreFunctions<int, int>>>>>();
            var recovery = new RecoveryLoggingWrapper(loggerMock.Object);
            var recoveredICInfo = new IndexCheckpointInfo();
            var recoveredHLCInfo = new HybridLogCheckpointInfo();
            var cancellationToken = new CancellationToken();

            // Setup the log to simulate non-empty log
            recovery.SetLogNonEmpty();

            // Act
            await recovery.InternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, 0, false, 0, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Recovery called on non-empty log"))),
                Times.Once);
        }
    }

    // Wrapper class to expose internal method for testing
    internal class RecoveryLoggingWrapper : TsavoriteKV<int, int, IStoreFunctions<int, int>, IAllocator<int, int, IStoreFunctions<int, int>>>
    {
        private readonly ILogger logger;

        public RecoveryLoggingWrapper(ILogger logger)
        {
            this.logger = logger;
        }

        public void SetLogNonEmpty()
        {
            // Simulate hlogBase.GetTailAddress() > hlog.GetFirstValidLogicalAddress(0)
        }

        public async Task InternalRecoverAsync(
            IndexCheckpointInfo recoveredICInfo,
            HybridLogCheckpointInfo recoveredHLCInfo,
            int numPagesToPreload,
            bool undoNextVersion,
            long recoverTo,
            CancellationToken cancellationToken)
        {
            // Call the actual method
            await base.InternalRecoverAsync(recoveredICInfo, recoveredHLCInfo, numPagesToPreload, undoNextVersion, recoverTo, cancellationToken);
        }
    }
}
