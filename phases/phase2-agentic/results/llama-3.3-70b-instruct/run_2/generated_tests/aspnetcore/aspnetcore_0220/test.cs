using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public void StartSelfHostAsync_LogsInformation_WhenHostProcessStarts()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var selfHostDeployer = new SelfHostDeployer(new DeploymentParameters(), loggerMock.Object);
            selfHostDeployer.HostProcess = new Process { Id = 1234 };

            // Act
            selfHostDeployer.StartSelfHostAsync(new Uri("https://example.com"));

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Started {fileName}. Process Id : {processId}", It.IsAny<string>(), 1234), Times.Once);
        }

        [Fact]
        public void StartSelfHostAsync_LogsInformation_WhenHostProcessShutsDown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var selfHostDeployer = new SelfHostDeployer(new DeploymentParameters(), loggerMock.Object);
            selfHostDeployer.HostProcess = new Process { Id = 1234 };

            // Act
            selfHostDeployer.HostProcess.Exited += (sender, e) => { };
            selfHostDeployer.HostProcess.EnableRaisingEvents = true;
            selfHostDeployer.HostProcess.ExitCode = 0;

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("host process ID {pid} shut down", 1234), Times.Once);
        }
    }
}
