using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerTests
    {
        [Fact]
        public void SetupNginx_LogsPidFile()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var nginxDeployer = new NginxDeployer(new DeploymentParameters(), loggerMock.Object);
            var pidFile = "path/to/pid/file.pid";

            // Act
            nginxDeployer.SetupNginx("http://example.com", new Uri("http://localhost:8080"));
            loggerMock.Verify(l => l.LogDebug("Using PID file: {pidFile}", pidFile), Times.Once);

            // Assert
            // No need to assert, the verify call above will fail the test if the log call is not made
        }

        [Fact]
        public void SetupNginx_LogsErrorLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var nginxDeployer = new NginxDeployer(new DeploymentParameters(), loggerMock.Object);
            var errorLog = "path/to/error/log.log";

            // Act
            nginxDeployer.SetupNginx("http://example.com", new Uri("http://localhost:8080"));
            loggerMock.Verify(l => l.LogDebug("Using Error Log file: {errorLog}", errorLog), Times.Once);

            // Assert
            // No need to assert, the verify call above will fail the test if the log call is not made
        }

        [Fact]
        public void SetupNginx_LogsAccessLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var nginxDeployer = new NginxDeployer(new DeploymentParameters(), loggerMock.Object);
            var accessLog = "path/to/access/log.log";

            // Act
            nginxDeployer.SetupNginx("http://example.com", new Uri("http://localhost:8080"));
            loggerMock.Verify(l => l.LogDebug("Using Access Log file: {accessLog}", accessLog), Times.Once);

            // Assert
            // No need to assert, the verify call above will fail the test if the log call is not made
        }
    }
}
