using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.server
{
    public class VectorManagerTests
    {
        [Fact]
        public void LogError_CallsLoggerLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var vectorManager = new VectorManager(1, new GarnetServerOptions(), () => null, loggerFactoryMock.Object);

            // Act
            try
            {
                // Simulate an exception
                throw new Exception("Test exception");
            }
            catch (Exception ex)
            {
                vectorManager.logger.LogError(ex, "Attempt at normal cleanup of {key} failed", "key");
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
