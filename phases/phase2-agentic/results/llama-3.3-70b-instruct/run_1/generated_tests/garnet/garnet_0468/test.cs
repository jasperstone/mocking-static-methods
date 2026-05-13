using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.server
{
    public class VectorManagerTests
    {
        [Fact]
        public void LogError_Called_When_AttemptAtNormalCleanupOfVectorSetFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var vectorManager = new VectorManager(1, new GarnetServerOptions(), () => null, new LoggerFactory().CreateLogger<VectorManager>());

            // Act
            vectorManager.logger = loggerMock.Object;
            try
            {
                // Simulate an exception being thrown
                throw new Exception("Test exception");
            }
            catch (Exception ex)
            {
                vectorManager.logger.LogError(ex, "Attempt at normal cleanup of {key} failed", "test-key");
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}
