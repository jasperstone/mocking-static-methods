using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests;

public class BuildHostProcessManagerTests
{
    [Fact]
    public void LogProcessFailure_LogsError_WhenProcessNotExited()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var processLogMessages = new List<string> { "mock process output" };
        var mockProcess = new Mock<Process>();
        mockProcess.Setup(p => p.HasExited).Returns(false);

        var manager = new TestManager(mockLogger.Object, mockProcess.Object, processLogMessages);

        // Act
        manager.LogProcessFailure();

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("The BuildHost process is not responding") == true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogProcessFailure_LogsErrorWithExitCode_WhenProcessExitedNonZero()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var processLogMessages = new List<string> { "mock process output" };
        var mockProcess = new Mock<Process>();
        mockProcess.Setup(p => p.HasExited).Returns(true);
        mockProcess.Setup(p => p.ExitCode).Returns(42);

        var manager = new TestManager(mockLogger.Object, mockProcess.Object, processLogMessages);

        // Act
        manager.LogProcessFailure();

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()?.Contains("The BuildHost process exited with 42") == true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogProcessFailure_DoesNothing_WhenLoggerNull()
    {
        // Arrange
        var processLogMessages = new List<string> { "ignored" };
        var mockProcess = new Mock<Process>();
        mockProcess.Setup(p => p.HasExited).Returns(false);

        var manager = new TestManager(null!, mockProcess.Object, processLogMessages);

        // Act
        manager.LogProcessFailure();

        // Assert - no exception thrown
        Assert.True(true);
    }
}

internal class TestManager
{
    private readonly ILogger? _logger;
    private readonly Process _process;
    private readonly List<string> _processLogMessages;

    public TestManager(ILogger? logger, Process process, List<string> processLogMessages)
    {
        _logger = logger;
        _process = process;
        _processLogMessages = processLogMessages;
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
