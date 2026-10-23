using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;

public class IndexRecoveryTests
{
    public class TestableTsavoriteBase : TsavoriteBase
    {
        public void TestAsyncPageReadCallback(uint errorCode, uint numBytes, object overlap)
        {
            AsyncPageReadCallback(errorCode, numBytes, overlap);
        }

        public ILogger logger
        {
            get { return base.logger; }
            set { base.logger = value; }
        }
    }

    [Fact]
    public void TestAsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var indexRecovery = new TestableTsavoriteBase();
        indexRecovery.logger = loggerMock.Object;

        // Act
        indexRecovery.TestAsyncPageReadCallback(1, 0, null);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public void TestAsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var indexRecovery = new TestableTsavoriteBase();
        indexRecovery.logger = loggerMock.Object;

        // Act
        indexRecovery.TestAsyncPageReadCallback(0, 0, null);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }
}
