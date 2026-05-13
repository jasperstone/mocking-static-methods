using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.AspNetCore.Server.IntegrationTesting.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class NginxDeployerTests
{
    [Fact]
    public async Task SetupNginx_LogsCorrectly()
    {
        // Arrange
        var deploymentParameters = new DeploymentParameters
        {
            ApplicationPath = Path.GetTempPath(),
            ServerConfigTemplateContent = "[user][errorlog][accesslog][listenPort][redirectUri][pidFile]"
        };
        var loggerFactory = new Mock<ILoggerFactory>();
        var logger = new Mock<ILogger<NginxDeployer>>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactory.Object);

        // Act
        await nginxDeployer.DeployAsync();

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using PID file:")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Exactly(3));
    }
}
