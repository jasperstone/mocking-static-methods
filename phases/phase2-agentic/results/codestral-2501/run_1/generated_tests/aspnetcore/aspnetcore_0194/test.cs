using System;
using System.IO;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerTests
    {
        [Fact]
        public void SetupNginx_LogsCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<NginxDeployer>>();
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = "/path/to/app",
                ServerConfigTemplateContent = "[user][errorlog][accesslog][listenPort][redirectUri][pidFile]"
            };

            var nginxDeployer = new NginxDeployer(deploymentParameters, Mock.Of<ILoggerFactory>())
            {
                Logger = loggerMock.Object
            };

            var redirectUri = "http://localhost:5000";
            var originalUri = new Uri("http://localhost:5001");

            // Act
            nginxDeployer.SetupNginx(redirectUri, originalUri);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using PID file:")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using Error Log file:")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using Access Log file:")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
