using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public async Task StartSelfHostAsync_LogsHostProcessShutdown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deploymentParameters = new DeploymentParameters();
            var selfHostDeployer = new SelfHostDeployer(deploymentParameters, new LoggerFactory());

            // Act
            await selfHostDeployer.StartSelfHostAsync(new Uri("http://localhost"));

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                "host process ID {pid} shut down",
                It.IsAny<object[]>()), Times.Once);
        }
    }
}
