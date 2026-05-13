using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public void StartSelfHostAsync_LogsHostProcessId()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var selfHostDeployer = new SelfHostDeployer(new DeploymentParameters(), new LoggerFactory());

            // Act
            selfHostDeployer.StartSelfHostAsync(new Uri("http://localhost:5000"));

            // Assert
            loggerMock.Verify(l => l.LogInformation("host process ID {pid} shut down", It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void StartSelfHostAsync_LogsStartedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var selfHostDeployer = new SelfHostDeployer(new DeploymentParameters(), new LoggerFactory());

            // Act
            selfHostDeployer.StartSelfHostAsync(new Uri("http://localhost:5000"));

            // Assert
            loggerMock.Verify(l => l.LogInformation("Started {fileName}. Process Id : {processId}", It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }
    }
}
