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
        public void SetupNginx_LogsTrace_WhenLogLevelIsTrace()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger<NginxDeployer>>();
            loggerFactory.Setup(f => f.CreateLogger<NginxDeployer>()).Returns(logger.Object);

            var deploymentParameters = new Mock<DeploymentParameters>();
            deploymentParameters.SetupGet(p => p.ApplicationPath).Returns(Path.GetTempPath());
            deploymentParameters.SetupGet(p => p.ServerConfigTemplateContent).Returns("server { listen 80; }");

            var nginxDeployer = new NginxDeployer(deploymentParameters.Object, loggerFactory.Object);

            var redirectUri = "http://localhost";
            var originalUri = new Uri("http://localhost:8080");

            // Act
            nginxDeployer.SetupNginx(redirectUri, originalUri);

            // Assert
            logger.Verify(
                l => l.LogTrace(
                    It.Is<string>(s => s.Contains("Config File Content:")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
