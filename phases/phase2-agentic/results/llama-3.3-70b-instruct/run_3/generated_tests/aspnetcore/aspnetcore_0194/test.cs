using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.AspNetCore.Server.IntegrationTesting
{
    public class NginxDeployerTests
    {
        [Fact]
        public void SetupNginx_LogsPidFile()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var nginxDeployer = new NginxDeployer(new DeploymentParameters(), new LoggerFactory());
            nginxDeployer.Logger = loggerMock.Object;
            var pidFile = "test.pid";

            // Act
            nginxDeployer.SetupNginx("http://example.com", new Uri("http://localhost:8080"));

            // Assert
            loggerMock.Verify(l => l.LogDebug("Using PID file: {pidFile}", pidFile), Times.Once);
        }

        [Fact]
        public void SetupNginx_LogsErrorLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var nginxDeployer = new NginxDeployer(new DeploymentParameters(), new LoggerFactory());
            nginxDeployer.Logger = loggerMock.Object;
            var errorLog = "test.error.log";

            // Act
            nginxDeployer.SetupNginx("http://example.com", new Uri("http://localhost:8080"));

            // Assert
            loggerMock.Verify(l => l.LogDebug("Using Error Log file: {errorLog}", errorLog), Times.Once);
        }

        [Fact]
        public void SetupNginx_LogsAccessLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var nginxDeployer = new NginxDeployer(new DeploymentParameters(), new LoggerFactory());
            nginxDeployer.Logger = loggerMock.Object;
            var accessLog = "test.access.log";

            // Act
            nginxDeployer.SetupNginx("http://example.com", new Uri("http://localhost:8080"));

            // Assert
            loggerMock.Verify(l => l.LogDebug("Using Access Log file: {accessLog}", accessLog), Times.Once);
        }
    }
}
