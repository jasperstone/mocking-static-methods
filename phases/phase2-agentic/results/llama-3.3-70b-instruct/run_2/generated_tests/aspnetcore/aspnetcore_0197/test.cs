using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting
{
    public class NginxDeployerTests
    {
        [Fact]
        public void SetupNginx_LogsConfigFileContentAtTraceLevel()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<NginxDeployer>();
            var mockLogger = new Mock<ILogger<NginxDeployer>>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var deploymentParameters = new DeploymentParameters
            {
                ServerConfigTemplateContent = "config content"
            };
            var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactory);

            // Act
            nginxDeployer.SetupNginx("redirectUri", new Uri("http://localhost"));

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void SetupNginx_DoesNotLogConfigFileContentAtTraceLevel_WhenTraceLevelIsDisabled()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<NginxDeployer>();
            var mockLogger = new Mock<ILogger<NginxDeployer>>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var deploymentParameters = new DeploymentParameters
            {
                ServerConfigTemplateContent = "config content"
            };
            var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactory);

            // Act
            nginxDeployer.SetupNginx("redirectUri", new Uri("http://localhost"));

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Never);
        }
    }
}
