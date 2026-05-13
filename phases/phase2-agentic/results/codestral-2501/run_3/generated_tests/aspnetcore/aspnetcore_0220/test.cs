using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.AspNetCore.Server.IntegrationTesting.Common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class SelfHostDeployerTests
{
    [Fact]
    public async Task DeployAsync_LogsInformation_WhenHostProcessStarts()
    {
        // Arrange
        var deploymentParameters = new DeploymentParameters();
        var loggerFactory = new Mock<ILoggerFactory>();
        var logger = new Mock<ILogger<SelfHostDeployer>>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var deployer = new SelfHostDeployer(deploymentParameters, loggerFactory.Object);

        // Act
        await deployer.DeployAsync();

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("host process ID")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task DeployAsync_LogsInformation_WhenHostProcessShutsDown()
    {
        // Arrange
        var deploymentParameters = new DeploymentParameters();
        var loggerFactory = new Mock<ILoggerFactory>();
        var logger = new Mock<ILogger<SelfHostDeployer>>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var deployer = new SelfHostDeployer(deploymentParameters, loggerFactory.Object);

        // Act
        await deployer.DeployAsync();

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("host process ID")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task DeployAsync_LogsInformation_WhenApplicationReady()
    {
        // Arrange
        var deploymentParameters = new DeploymentParameters();
        var loggerFactory = new Mock<ILoggerFactory>();
        var logger = new Mock<ILogger<SelfHostDeployer>>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var deployer = new SelfHostDeployer(deploymentParameters, loggerFactory.Object);

        // Act
        await deployer.DeployAsync();

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Application ready at URL:")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
