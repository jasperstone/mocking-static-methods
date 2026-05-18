using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class BuildHostProcessManagerTests
{
    [Fact]
    public void LogProcessFailure_ProcessNotResponding_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
        var processMock = new Mock<Process>();
        processMock.Setup(p => p.HasExited).Returns(false);
        processMock.Setup(p => p.ExitCode).Returns(0);

        var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerMock.Object);
        var buildHostProcessManagerType = typeof(BuildHostProcessManager);
        var processField = buildHostProcessManagerType.GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        processField.SetValue(buildHostProcessManager, processMock.Object);

        // Act
        buildHostProcessManager.LogProcessFailure();

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                "The BuildHost process is not responding. Process output:{newLine}{processLog}",
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public void LogProcessFailure_ProcessExitedWithError_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
        var processMock = new Mock<Process>();
        processMock.Setup(p => p.HasExited).Returns(true);
        processMock.Setup(p => p.ExitCode).Returns(1);

        var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: loggerMock.Object);
        var buildHostProcessManagerType = typeof(BuildHostProcessManager);
        var processField = buildHostProcessManagerType.GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        processField.SetValue(buildHostProcessManager, processMock.Object);

        // Act
        buildHostProcessManager.LogProcessFailure();

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                "The BuildHost process exited with {errorCode}. Process output:{newLine}{processLog}",
                It.IsAny<object[]>()),
            Times.Once);
    }
}
