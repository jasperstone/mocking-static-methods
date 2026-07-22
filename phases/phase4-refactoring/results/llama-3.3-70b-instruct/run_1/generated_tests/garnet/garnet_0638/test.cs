using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class IndexRecoveryTests
{
    [Fact]
    public void TestAsyncPageReadCallback_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var indexRecovery = new TsavoriteBase();
        indexRecovery.logger = loggerMock.Object;

        // Act
        indexRecovery.AsyncPageReadCallback(1, 0, null);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }
}
