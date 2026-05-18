using System;
using System.IO;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class NginxDeployerTests
{
    [Fact]
    public void SetupNginx_LogsDebugMessages()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<NginxDeployer>>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

        var deploymentParameters = new Mock<DeploymentParameters>();
        deploymentParameters.Setup(x => x.ApplicationPath).Returns("/path/to/app");
        deploymentParameters.Setup(x => x.ServerConfigTemplateContent).Returns("[user][errorlog][accesslog][listenPort][redirectUri][pidFile]");

        var nginxDeployer = new NginxDeployer(deploymentParameters.Object, mockLoggerFactory.Object);

        var redirectUri = "http://localhost:5000";
        var originalUri = new Uri("http://localhost:5001");

        // Act
        nginxDeployer.SetupNginx(redirectUri, originalUri);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Using PID file:")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Exactly(3));
    }
}
