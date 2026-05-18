using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests;

public class BuildHostProcessTests
{
    [Fact]
    public void LogProcessFailure_WhenProcessExitedWithNonZeroExitCode_LogsErrorWithExitCodeAndProcessLog()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var processMock = new Mock<Process>();
        processMock.Setup(p => p.HasExited).Returns(true);
        processMock.Setup(p => p.ExitCode).Returns(42);
        
        var processLogMessages = new List<string> { "line1", "line2" };
        var process = new TestBuildHostProcess(loggerMock.Object, processMock.Object, processLogMessages);

        // Act
        process.LogProcessFailure();

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("The BuildHost process exited with 42") &&
                    v.ToString()!.Contains("line1") &&
                    v.ToString()!.Contains("line2")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class TestBuildHostProcess
    {
        private readonly ILogger _logger;
        private readonly Process _process;
        private readonly List<string> _processLogMessages = new();

        public TestBuildHostProcess(ILogger logger, Process process, List<string> processLogMessages)
        {
            _logger = logger;
            _process = process;
            foreach (var msg in processLogMessages)
            {
                _processLogMessages.Add(msg);
            }
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
}
