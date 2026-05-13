using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
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
            var deploymentParameters = new DeploymentParameters();
            deploymentParameters.ServerConfigTemplateContent = "config content";
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
    }
}
