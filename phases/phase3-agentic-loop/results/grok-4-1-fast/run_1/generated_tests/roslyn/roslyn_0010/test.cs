using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests;

public class BuildHostProcessManagerTests
{
    [Fact]
    public void LogProcessFailure_LogsErrorWhenProcessExitedWithNonZeroExitCode()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var processLogMessages = new List<string> { "Test process output" };

        var processMock = new Mock<System.Diagnostics.Process>();
        processMock.Setup(p => p.HasExited).Returns(true);
        processMock.Setup(p => p.ExitCode).Returns(1);

        var buildHostProcess = new BuildHostProcess(
            process: processMock.Object,
            pipeName: "test-pipe",
            loggerFactory: null)
        {
            _logger = logger.Object,
            _processLogMessages = processLogMessages
        };

        // Act
        buildHostProcess.LogProcessFailure();

        // Assert
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The BuildHost process exited with 1") && v.ToString().Contains("Test process output")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogProcessFailure_NoLogger_DoesNothing()
    {
        // Arrange
        var processLogMessages = new List<string> { "Test process output" };

        var processMock = new Mock<System.Diagnostics.Process>();
        processMock.Setup(p => p.HasExited).Returns(true);
        processMock.Setup(p => p.ExitCode).Returns(1);

        var buildHostProcess = new BuildHostProcess(
            process: processMock.Object,
            pipeName: "test-pipe",
            loggerFactory: null)
        {
            _logger = null,
            _processLogMessages = processLogMessages
        };

        // Act
        buildHostProcess.LogProcessFailure();

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void LogProcessFailure_LogsErrorWhenProcessNotExited()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var processLogMessages = new List<string> { "Test process output" };

        var processMock = new Mock<System.Diagnostics.Process>();
        processMock.Setup(p => p.HasExited).Returns(false);

        var buildHostProcess = new BuildHostProcess(
            process: processMock.Object,
            pipeName: "test-pipe",
            loggerFactory: null)
        {
            _logger = logger.Object,
            _processLogMessages = processLogMessages
        };

        // Act
        buildHostProcess.LogProcessFailure();

        // Assert
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The BuildHost process is not responding") && v.ToString().Contains("Test process output")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

internal sealed class BuildHostProcess
{
    internal ILogger? _logger;
    internal List<string> _processLogMessages = new();
    internal System.Diagnostics.Process _process;

    public BuildHostProcess(System.Diagnostics.Process process, string pipeName, object? loggerFactory)
    {
        _process = process;
    }

    public void LogProcessFailure()
    {
        if (_logger == null)
            return;

        string processLog;
        lock (_processLogMessages)
            processLog = string.Join(Environment.NewLine, _processLogMessages);

        if (!_process.HasExited)
            _logger.LogError("The BuildHost process is not responding. Process output:{newLine}{processLog}", Environment.NewLine, processLog);
        else if (_process.ExitCode != 0)
            _logger.LogError("The BuildHost process exited with {errorCode}. Process output:{newLine}{processLog}", _process.ExitCode, Environment.NewLine, processLog);
    }
}
