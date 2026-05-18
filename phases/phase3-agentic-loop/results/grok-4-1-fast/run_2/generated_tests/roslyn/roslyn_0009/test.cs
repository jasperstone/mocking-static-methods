using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Microsoft.CodeAnalysis.Test.Utilities;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests;

[Trait(TraitConstants.Feature, TraitConstants.MSBuild)]
public class BuildHostProcessManagerTests
{
    [Fact]
    public void LogProcessFailure_LogsError_WhenProcessHasNotExited()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var process = new Mock<Process>();
        process.Setup(p => p.HasExited).Returns(false);
        var processLogMessages = new StringBuilder("Test process output");

        var testInstance = new TestBuildHostProcessManager(logger.Object, process.Object, processLogMessages);

        // Act
        testInstance.CallLogProcessFailure();

        // Assert
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType<string>>((v, t) => 
                    v.ToString()!.Contains("The BuildHost process is not responding. Process output:") &&
                    v.ToString()!.Contains("Test process output")),
                null,
                It.IsAny<Func<It.IsAnyType<string>, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogProcessFailure_LogsErrorWithExitCode_WhenProcessExitedNonZero()
    {
        // Arrange
        var logger = new Mock<ILogger>();
        var process = new Mock<Process>();
        process.Setup(p => p.HasExited).Returns(true);
        process.Setup(p => p.ExitCode).Returns(123);
        var processLogMessages = new StringBuilder("Test process output");

        var testInstance = new TestBuildHostProcessManager(logger.Object, process.Object, processLogMessages);

        // Act
        testInstance.CallLogProcessFailure();

        // Assert
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType<string>>((v, t) => 
                    v.ToString()!.Contains("The BuildHost process exited with 123") &&
                    v.ToString()!.Contains("Test process output")),
                null,
                It.IsAny<Func<It.IsAnyType<string>, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogProcessFailure_DoesNothing_WhenLoggerIsNull()
    {
        // Arrange
        var process = new Mock<Process>();
        process.Setup(p => p.HasExited).Returns(false);
        var processLogMessages = new StringBuilder();

        var testInstance = new TestBuildHostProcessManager(null!, process.Object, processLogMessages);

        // Act & Assert
        var exception = Record.Exception(() => testInstance.CallLogProcessFailure());
        Assert.Null(exception);
    }
}

internal sealed class TestBuildHostProcessManager : Microsoft.CodeAnalysis.MSBuild.BuildHostProcessManager
{
    private readonly ILogger? _logger;
    private readonly Process _process;
    private readonly StringBuilder _processLogMessages;

    public TestBuildHostProcessManager(ILogger? logger, Process process, StringBuilder processLogMessages)
    {
        _logger = logger;
        _process = process;
        _processLogMessages = processLogMessages;
    }

    protected override void LogProcessFailure()
    {
        if (_logger == null)
            return;

        string processLog;
        lock (_processLogMessages)
            processLog = _processLogMessages.ToString();

        if (!_process.HasExited)
            _logger.LogError("The BuildHost process is not responding. Process output:{newLine}{processLog}", Environment.NewLine, processLog);
        else if (_process.ExitCode != 0)
            _logger.LogError("The BuildHost process exited with {errorCode}. Process output:{newLine}{processLog}", _process.ExitCode, Environment.NewLine, processLog);
    }

    public void CallLogProcessFailure() => LogProcessFailure();
}
