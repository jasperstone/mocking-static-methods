using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerTests
    {
        [Fact]
        public void SetupNginx_LogsConfigFileContentAtTraceLevel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deploymentParameters = new DeploymentParameters();
            var loggerFactory = new LoggerFactory();
            var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactory);
            nginxDeployer.Logger = loggerMock.Object;
            deploymentParameters.ServerConfigTemplateContent = "Test config content";

            // Act
            nginxDeployer.SetupNginx("http://example.com", new Uri("http://localhost:8080"));

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)((state, exception) => state.ToString())
            ), Times.Once);
        }

        [Fact]
        public void SetupNginx_DoesNotLogConfigFileContentAtNonTraceLevel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deploymentParameters = new DeploymentParameters();
            var loggerFactory = new LoggerFactory();
            var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactory);
            nginxDeployer.Logger = loggerMock.Object;
            deploymentParameters.ServerConfigTemplateContent = "Test config content";
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

            // Act
            nginxDeployer.SetupNginx("http://example.com", new Uri("http://localhost:8080"));

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)((state, exception) => state.ToString())
            ), Times.Never);
        }
    }
}
