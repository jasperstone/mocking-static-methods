using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.Tests
{
    public class RecoveryTests
    {
        [Fact]
        public void LogInformation_ShouldBeCalled_WhenRecoveryCalledOnNonEmptyLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recovery = new Recovery(loggerMock.Object);

            // Simulate a non-empty log condition
            recovery.hlogBase = new Mock<HybridLogBase>().Object;
            recovery.hlogBase.GetTailAddress().Returns(100L); // Simulate non-empty log
            recovery.hlog = new Mock<HybridLog>().Object;
            recovery.hlog.GetFirstValidLogicalAddress(0).Returns(0L);

            // Act
            recovery.InternalRecoverAsync(new IndexCheckpointInfo(), new HybridLogCheckpointInfo(), 0, false, 0, CancellationToken.None).GetAwaiter().GetResult();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store."),
                Times.Once);
        }
    }
}
