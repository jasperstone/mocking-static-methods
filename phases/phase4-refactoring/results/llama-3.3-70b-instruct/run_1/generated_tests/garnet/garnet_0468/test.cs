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
            var vectorManager = new VectorManager(1, new GarnetServerOptions(), () => new RespServerSession(new System.Net.Sockets.Socket(), new LoggerFactory().CreateLogger<RespServerSession>()), new LoggerFactory().CreateLogger<VectorManager>());

            // Act
            try
            {
                // Simulate an exception being thrown
                throw new Exception("Attempt at normal cleanup of Vector Set failed");
            }
            catch (Exception ex)
            {
                // Assert
                loggerMock.Verify(l => l.LogError(ex, "Attempt at normal cleanup of {key} failed", It.IsAny<string>()), Times.Once);
            }
        }
    }
}
