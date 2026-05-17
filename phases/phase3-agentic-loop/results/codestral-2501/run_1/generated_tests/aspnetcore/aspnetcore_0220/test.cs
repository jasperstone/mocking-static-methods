using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class SelfHostDeployerTests
{
    [Fact]
    public async Task DeployAsync_LogsCorrectInformation()
    {
        // Arrange
        var deploymentParameters = new Mock<DeploymentParameters>();
        var loggerFactory = new Mock<ILoggerFactory>();
        var logger = new Mock<ILogger<SelfHostDeployer>>();
        var selfHostDeployer = new SelfHostDeployer(deploymentParameters.Object, loggerFactory.Object);

        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        // Act
        await selfHostDeployer.DeployAsync();

        // Assert
        logger.Verify(x => x.LogInformation("Started {fileName}. Process Id : {processId}", It.IsAny<string>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void Dispose_CleansUpResources()
    {
        // Arrange
        var deploymentParameters = new Mock<DeploymentParameters>();
        var loggerFactory = new Mock<ILoggerFactory>();
        var logger = new Mock<ILogger<SelfHostDeployer>>();
        var selfHostDeployer = new SelfHostDeployer(deploymentParameters.Object, loggerFactory.Object);

        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        // Act
        selfHostDeployer.Dispose();

        // Assert
        logger.Verify(x => x.LogInformation("Attempting to cancel process {0}", It.IsAny<int>()), Times.Once);
    }
}
