using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;

public class RecoveryTests
{
    [Fact]
    public void LogInformation_Called_When_Recovery_Called_On_Non_Empty_Log()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var recovery = new TsavoriteKV<object, object, object, object>();

        // Act
        recovery.InternalRecoverAsync(new IndexCheckpointInfo(), new HybridLogCheckpointInfo(), 0, false, 0, CancellationToken.None);

        // Assert
        mockLogger.Verify(logger => logger.LogInformation("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store."), Times.Once);
    }
}
