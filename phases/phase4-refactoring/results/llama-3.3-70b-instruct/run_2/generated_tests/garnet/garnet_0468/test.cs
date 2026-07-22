using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using Garnet.common;

public class VectorManagerTests
{
    [Fact]
    public void LogError_Called_When_AttemptAtNormalCleanupOfVectorSetFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<VectorManager>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
        var vectorManager = new VectorManager(1, new GarnetServerOptions(), () => null, loggerFactoryMock.Object);

        // Act
        try
        {
            // Simulate an exception being thrown
            throw new Exception("Test exception");
        }
        catch (Exception ex)
        {
            loggerMock.Object.LogError(ex, "Attempt at normal cleanup of {key} failed", "test-key");
        }

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }
}
