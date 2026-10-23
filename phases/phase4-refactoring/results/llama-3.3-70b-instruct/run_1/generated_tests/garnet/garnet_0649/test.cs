using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;

public class RecoveryTests
{
    [Fact]
    public void LogInformation_Called_When_RecoveryCalledOnNonEmptyLog()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var recovery = new TsavoriteKV<int, int, IStoreFunctions<int, int>, IAllocator<int, int, IStoreFunctions<int, int>>>(loggerMock.Object);

        // Act
        recovery.InternalRecoverAsync(new IndexCheckpointInfo(), new HybridLogCheckpointInfo(), 0, false, 0, default);

        // Assert
        loggerMock.Verify(l => l.LogInformation("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store."), Times.Once);
    }
}
