using System;
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
                ServerConfigTemplateContent = "nginx config content"
            };

            _nginxDeployer = new NginxDeployer(_deploymentParameters, _loggerFactoryMock.Object);
        }

        [Fact]
        public void SetupNginx_LogsTrace_WhenTraceIsEnabled()
        {
            // Arrange
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(true);

            // Act
            _nginxDeployer.GetType().GetMethod("SetupNginx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_nginxDeployer, new object[] { "http://localhost:5000", new Uri("http://localhost:5000") });

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Config File Content")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public void SetupNginx_DoesNotLogTrace_WhenTraceIsDisabled()
        {
            // Arrange
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            _nginxDeployer.GetType().GetMethod("SetupNginx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_nginxDeployer, new object[] { "http://localhost:5000", new Uri("http://localhost:5000") });

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Config File Content")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Never);
        }
    }
}
