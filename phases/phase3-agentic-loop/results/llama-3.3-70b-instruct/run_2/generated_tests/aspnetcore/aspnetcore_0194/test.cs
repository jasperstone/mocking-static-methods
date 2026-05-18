using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerTests
    {
        [Fact]
        public void SetupNginx_LogsPidFile()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<NginxDeployer>();
            var mockLogger = new Mock<ILogger<NginxDeployer>>();
            mockLogger.Setup(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object>())).Verifiable();
            var deploymentParameters = new DeploymentParameters();
            deploymentParameters.ApplicationPath = Path.GetTempPath();
            deploymentParameters.ServerConfigTemplateContent = "template";
            var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactory);
            nginxDeployer.Logger = mockLogger.Object;
            var pidFile = Path.Combine(deploymentParameters.ApplicationPath, $"{Guid.NewGuid()}.nginx.pid");

            // Act
            nginxDeployer.SetupNginx("http://localhost:5000", new Uri("http://localhost:5000"));

            // Assert
            mockLogger.Verify(l => l.LogDebug("Using PID file: {pidFile}", pidFile), Times.Once);
        }

        [Fact]
        public void SetupNginx_LogsErrorLog()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<NginxDeployer>();
            var mockLogger = new Mock<ILogger<NginxDeployer>>();
            mockLogger.Setup(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object>())).Verifiable();
            var deploymentParameters = new DeploymentParameters();
            deploymentParameters.ApplicationPath = Path.GetTempPath();
            deploymentParameters.ServerConfigTemplateContent = "template";
            var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactory);
            nginxDeployer.Logger = mockLogger.Object;
            var errorLog = Path.Combine(deploymentParameters.ApplicationPath, "nginx.error.log");

            // Act
            nginxDeployer.SetupNginx("http://localhost:5000", new Uri("http://localhost:5000"));

            // Assert
            mockLogger.Verify(l => l.LogDebug("Using Error Log file: {errorLog}", errorLog), Times.Once);
        }

        [Fact]
        public void SetupNginx_LogsAccessLog()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<NginxDeployer>();
            var mockLogger = new Mock<ILogger<NginxDeployer>>();
            mockLogger.Setup(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object>())).Verifiable();
            var deploymentParameters = new DeploymentParameters();
            deploymentParameters.ApplicationPath = Path.GetTempPath();
            deploymentParameters.ServerConfigTemplateContent = "template";
            var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactory);
            nginxDeployer.Logger = mockLogger.Object;
            var accessLog = Path.Combine(deploymentParameters.ApplicationPath, "nginx.access.log");

            // Act
            nginxDeployer.SetupNginx("http://localhost:5000", new Uri("http://localhost:5000"));

            // Assert
            mockLogger.Verify(l => l.LogDebug("Using Access Log file: {accessLog}", accessLog), Times.Once);
        }
    }
}
