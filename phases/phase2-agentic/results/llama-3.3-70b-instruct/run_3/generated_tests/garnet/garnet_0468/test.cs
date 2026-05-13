using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.server.Tests
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
            try
            {
                vectorManager.ResumePostRecovery();
            }
            catch (Exception ex)
            {
                loggerMock.Object.LogError(ex, "Attempt at normal cleanup of {key} failed", "testKey");
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
