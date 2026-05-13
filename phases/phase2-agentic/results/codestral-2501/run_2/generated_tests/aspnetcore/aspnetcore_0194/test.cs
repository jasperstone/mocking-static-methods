using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.AspNetCore.Server.IntegrationTesting.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerTests
    {
        private readonly Mock<ILogger<NginxDeployer>> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly DeploymentParameters _deploymentParameters;
        private readonly NginxDeployer _nginxDeployer;

        public NginxDeployerTests()
        {
            _loggerMock = new Mock<ILogger<NginxDeployer>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
            _deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = Path.GetTempPath(),
                ServerConfigTemplateContent = "[user][errorlog][accesslog][listenPort][redirectUri][pidFile]"
            };
            _nginxDeployer = new NginxDeployer(_deploymentParameters, _loggerFactoryMock.Object);
        }

        [Fact]
        public void SetupNginx_LogsDebugMessages()
        {
            // Arrange
            var redirectUri = "http://localhost:5000";
            var originalUri = new Uri("http://localhost:5001");

            // Act
            _nginxDeployer.SetupNginx(redirectUri, originalUri);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using PID file:")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(3));
        }

        [Fact]
        public void SetupNginx_ReplacesTemplateContent()
        {
            // Arrange
            var redirectUri = "http://localhost:5000";
            var originalUri = new Uri("http://localhost:5001");

            // Act
            _nginxDeployer.SetupNginx(redirectUri, originalUri);

            // Assert
            Assert.Contains("localhost:5001", _deploymentParameters.ServerConfigTemplateContent);
            Assert.Contains(redirectUri, _deploymentParameters.ServerConfigTemplateContent);
        }

        [Fact]
        public void SetupNginx_StartsNginxProcess()
        {
            // Arrange
            var redirectUri = "http://localhost:5000";
            var originalUri = new Uri("http://localhost:5001");

            // Act
            _nginxDeployer.SetupNginx(redirectUri, originalUri);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("nginx process ID")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
