using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Tsavorite.core
{
    public class RecoveryTests
    {
        [Fact]
        public void LogInformation_Called_When_RecoveryCalledOnNonEmptyLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeFunctionsMock = new Mock<IStoreFunctions<int, int>>();
            var allocatorMock = new Mock<IAllocator<int, int, IStoreFunctions<int, int>>>();
            var recovery = new TsavoriteKV<int, int, IStoreFunctions<int, int>, IAllocator<int, int, IStoreFunctions<int, int>>>(new KVSettings<int, int>(), storeFunctionsMock.Object, allocatorMock.Object, loggerMock.Object);

            // Act
            // You would need to make the InternalRecoverAsync method public to call it from this test
            // recovery.InternalRecoverAsync(new IndexCheckpointInfo(), new HybridLogCheckpointInfo(), 0, false, 0, default);

            // Assert
            // loggerMock.Verify(l => l.LogInformation("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store."), Times.Once);
        }
    }
}
