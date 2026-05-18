using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.server.Tests
{
    public class ServerConfigTests
    {
        [Fact]
        public void TestLogWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var serverConfig = new RespServerSession(loggerMock.Object);

            // Act
            serverConfig.NetworkCONFIG_SET();

            // Assert
            loggerMock.Verify(l => l.LogWarning("Cluster username is not provided, will use new password with existing username"), Times.Once);
        }
    }
}
