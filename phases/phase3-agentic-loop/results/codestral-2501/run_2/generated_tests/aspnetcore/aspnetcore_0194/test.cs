using System;
using System.IO;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class NginxDeployerTests
{
    [Fact]
    public void SetupNginx_LogsCorrectly()
    {
        // Arrange
        var deploymentParameters = new Mock<DeploymentParameters>();
        deploymentParameters.Setup(p => p.ApplicationPath).Returns("/path/to/app");
        deploymentParameters.Setup(p => p.ServerConfigTemplateContent).Returns("[user][errorlog][accesslog][listenPort][redirectUri][pidFile]");

        var loggerMock = new Mock<ILogger<NginxDeployer>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var nginxDeployer = new NginxDeployer(deploymentParameters.Object, loggerFactoryMock.Object);

        var redirectUri = "http://localhost:5000";
        var originalUri = new Uri("http://localhost:5001");

        // Act
        nginxDeployer.SetupNginx(redirectUri, originalUri);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Exactly(3));
    }
}
