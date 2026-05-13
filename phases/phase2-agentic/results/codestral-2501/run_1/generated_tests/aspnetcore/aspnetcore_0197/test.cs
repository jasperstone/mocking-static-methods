using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
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
                ApplicationPath = "/tmp",
                ServerConfigTemplateContent = "[user][errorlog][accesslog][listenPort][redirectUri][pidFile]"
            };
            _nginxDeployer = new NginxDeployer(_deploymentParameters, _loggerFactoryMock.Object);
        }

        [Fact]
        public void SetupNginx_LogsConfigContent_WhenTraceIsEnabled()
        {
            // Arrange
            var redirectUri = "http://localhost:5000";
            var originalUri = new Uri("http://localhost:5001");
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            _nginxDeployer.SetupNginx(redirectUri, originalUri);

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Config File Content")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void SetupNginx_ThrowsInvalidOperationException_WhenUserNameIsNull()
        {
            // Arrange
            var redirectUri = "http://localhost:5000";
            var originalUri = new Uri("http://localhost:5001");
            Environment.SetEnvironmentVariable("LOGNAME", null);
            Environment.SetEnvironmentVariable("USER", null);
            Environment.SetEnvironmentVariable("USERNAME", null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _nginxDeployer.SetupNginx(redirectUri, originalUri));
        }

        [Fact]
        public void SetupNginx_LogsWarning_WhenPidFileDoesNotExist()
        {
            // Arrange
            var redirectUri = "http://localhost:5000";
            var originalUri = new Uri("http://localhost:5001");
            var pidFile = Path.Combine(_deploymentParameters.ApplicationPath, "non_existent.pid");
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            _nginxDeployer.SetupNginx(redirectUri, originalUri);

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to find nginx PID file")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
