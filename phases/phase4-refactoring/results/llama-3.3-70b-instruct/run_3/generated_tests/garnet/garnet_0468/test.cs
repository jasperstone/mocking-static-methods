using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.server
{
    public class VectorManagerTests
    {
        [Fact]
        public void ResumePostRecovery_LogErrorCalled_WhenDeleteFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var vectorManager = new VectorManager(1, new GarnetServerOptions(), () => new RespServerSession(), loggerMock.Object);

            // Act
            vectorManager.ResumePostRecovery();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Attempt at normal cleanup of {key} failed", It.IsAny<string>()), Times.Once);
        }
    }
}
