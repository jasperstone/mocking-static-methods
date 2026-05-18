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
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var vectorManager = new VectorManager(1, new GarnetServerOptions(), () => null, loggerFactoryMock.Object);

            // Act
            try
            {
                // Simulate a failed attempt at normal cleanup of a Vector Set
                vectorManager.ResumePostRecovery();
            }
            catch (Exception ex)
            {
                // Assert
                loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Attempt at normal cleanup of {key} failed", It.IsAny<string>()), Times.Once);
            }
        }
    }
}
