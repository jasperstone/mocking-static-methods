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
    public void LogTrace_Called_When_Trace_Enabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NginxDeployer>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var deploymentParameters = new DeploymentParameters
        {
            ApplicationPath = Path.GetTempPath(),
            ServerConfigTemplateContent = "Template content"
        };

        var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactoryMock.Object);

        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

        // Act
        nginxDeployer.SetupNginx("http://redirect", new Uri("http://original"));

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Config File Content")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogTrace_NotCalled_When_Trace_Disabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NginxDeployer>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var deploymentParameters = new DeploymentParameters
        {
            ApplicationPath = Path.GetTempPath(),
            ServerConfigTemplateContent = "Template content"
        };

        var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactoryMock.Object);

        loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);

        // Act
        nginxDeployer.SetupNginx("http://redirect", new Uri("http://original"));

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Config File Content")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Never);
    }
}
