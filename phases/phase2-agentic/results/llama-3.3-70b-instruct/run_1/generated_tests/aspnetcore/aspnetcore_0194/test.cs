using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerTests
    {
        [Fact]
        public void SetupNginx_LogsDebugMessages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deploymentParameters = new DeploymentParameters();
            var nginxDeployer = new NginxDeployer(deploymentParameters, new LoggerFactory());

            // Act
            nginxDeployer.SetupNginx("http://example.com", new Uri("http://localhost:5000"));

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<FormattedLogValues>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<FormattedLogValues, Exception, string>>()),
                Times.Exactly(3));
        }
    }
}
