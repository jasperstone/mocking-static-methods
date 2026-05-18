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
    public async Task DeployAsync_LogsInformation_WhenHostProcessStarts()
    {
        // Arrange
        var deploymentParameters = new DeploymentParameters
        {
            ApplicationPath = "path/to/application",
            ApplicationName = "app",
            ServerType = ServerType.Kestrel,
            Scheme = "http",
            ApplicationBaseUriHint = "http://localhost",
            StatusMessagesEnabled = true,
            RuntimeFlavor = RuntimeFlavor.CoreClr,
            ApplicationType = ApplicationType.Standalone,
            TargetFramework = "net5.0",
            Configuration = "Debug",
            EnvironmentName = "Development",
            EnvironmentVariables = new System.Collections.Generic.Dictionary<string, string>()
        };

        var loggerFactory = new Mock<ILoggerFactory>();
        var logger = new Mock<ILogger<SelfHostDeployer>>();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

        var selfHostDeployer = new SelfHostDeployer(deploymentParameters, loggerFactory.Object);

        var process = new Mock<Process>();
        process.Setup(p => p.StartAndCaptureOutAndErrToLogger(It.IsAny<string>(), It.IsAny<ILogger>())).Verifiable();
        process.Setup(p => p.HasExited).Returns(false);
        process.Setup(p => p.Id).Returns(12345);
        process.Setup(p => p.OutputDataReceived += It.IsAny<DataReceivedEventHandler>()).Verifiable();
        process.Setup(p => p.Exited += It.IsAny<EventHandler>()).Verifiable();

        selfHostDeployer.HostProcess = process.Object;

        // Act
        await selfHostDeployer.DeployAsync();

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Started {fileName}. Process Id : {processId}")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
