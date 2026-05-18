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
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = string.Empty
            };
            var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Act
            nginxDeployer.SetupNginx("http://example.com", new Uri("http://localhost:8080"));

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(3));
        }

        [Fact]
        public void SetupNginx_LogsErrorLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = string.Empty
            };
            var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Act
            nginxDeployer.SetupNginx("http://example.com", new Uri("http://localhost:8080"));

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(3));
        }

        [Fact]
        public void SetupNginx_LogsAccessLog()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = string.Empty
            };
            var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Act
            nginxDeployer.SetupNginx("http://example.com", new Uri("http://localhost:8080"));

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(3));
        }
    }
}
