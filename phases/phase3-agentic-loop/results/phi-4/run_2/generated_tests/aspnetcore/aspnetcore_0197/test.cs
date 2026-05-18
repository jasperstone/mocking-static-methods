using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerTests
    {
        [Fact]
        public void SetupNginx_LogsTraceMessage_WhenLogLevelIsEnabled()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger<NginxDeployer>>();
            loggerFactory.Setup(f => f.CreateLogger<NginxDeployer>()).Returns(logger.Object);

            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = "server { listen 80; }"
            };

            var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactory.Object)
            {
                Logger = logger.Object
            };

            var redirectUri = "http://localhost";
            var originalUri = new Uri("http://localhost:8080");

            // Act
            nginxDeployer.SetupNginx(redirectUri, originalUri);

            // Assert
            logger.Verify(
                l => l.IsEnabled(LogLevel.Trace),
                Times.Once);

            logger.Verify(
                l => l.LogTrace(
                    It.Is<string>(s => s.Contains("Config File Content:")),
                    deploymentParameters.ServerConfigTemplateContent),
                Times.Once);
        }
    }
}
