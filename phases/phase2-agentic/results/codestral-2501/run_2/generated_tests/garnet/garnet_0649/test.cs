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
        var loggerMock = new Mock<ILogger<TsavoriteKV<int, int, IStoreFunctions<int, int>, IAllocator<int, int, IStoreFunctions<int, int>>>>();
        var recovery = new TsavoriteKV<int, int, IStoreFunctions<int, int>, IAllocator<int, int, IStoreFunctions<int, int>>();
        recovery.logger = loggerMock.Object;

        // Act
        recovery.InternalRecoverAsync(new IndexCheckpointInfo(), new HybridLogCheckpointInfo(), 0, false, 0, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store.")),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
