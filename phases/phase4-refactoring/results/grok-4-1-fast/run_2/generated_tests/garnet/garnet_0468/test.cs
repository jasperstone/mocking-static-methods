using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.server.Tests
{
    public class VectorManagerTests
    {
        [Fact]
        public void ResumePostRecovery_LogsError_OnTryDeleteVectorSetException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VectorManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.Is<string>(s => s.Contains("VectorManager"))))
                           .Returns(loggerMock.Object);

            // Mock cleanup session factory to return null (triggers exception path)
            Func<Garnet.networking.IMessageConsumer> getCleanupSession = () => null;

            var serverOptions = new GarnetServerOptions 
            { 
                EnableVectorSetPreview = true,
                VectorSetReplayTaskCount = 1
            };

            var vectorManager = new VectorManager(0, serverOptions, getCleanupSession, loggerFactoryMock.Object);

            // Setup verification for the specific LogError call on line 221
            loggerMock.Setup(l => l.LogError(
                It.IsAny<Exception>(),
                "Attempt at normal cleanup of {key} failed",
                It.IsAny<object[]>()
            )).Verifiable();

            // Act
            vectorManager.ResumePostRecovery();

            // Assert - verify the LogError extension call was made with correct message
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "Attempt at normal cleanup of {key} failed",
                    It.IsAny<object[]>()
                ),
                Times.AtLeastOnce
            );
        }
    }
}
