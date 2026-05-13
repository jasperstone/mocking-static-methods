using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        private class TestBuildHostProcess
        {
            public Mock<Process> ProcessMock { get; }
            public StringBuilder ProcessLogMessages { get; }
            public ILogger Logger { get; }

            public TestBuildHostProcess(int exitCode, bool hasExited, string logMessages, ILogger logger)
            {
                ProcessMock = new Mock<Process>();
                ProcessMock.Setup(p => p.HasExited).Returns(hasExited);
                ProcessMock.Setup(p => p.ExitCode).Returns(exitCode);
                ProcessLogMessages = new StringBuilder(logMessages);
                Logger = logger;
            }

            public void LogProcessFailure()
            {
                if (Logger == null)
                    return;

                string processLog;
                lock (ProcessLogMessages)
                    processLog = ProcessLogMessages.ToString();

                if (!ProcessMock.Object.HasExited)
                    Logger.LogError("The BuildHost process is not responding. Process output:{newLine}{processLog}", Environment.NewLine, processLog);
                else if (ProcessMock.Object.ExitCode != 0)
                    Logger.LogError("The BuildHost process exited with {errorCode}. Process output:{newLine}{processLog}", ProcessMock.Object.ExitCode, Environment.NewLine, processLog);
            }
        }

        [Fact]
        public void LogProcessFailure_LogsError_WhenProcessNotExited()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var logMessages = "Some process output";
            var testProcess = new TestBuildHostProcess(exitCode: 0, hasExited: false, logMessages: logMessages, logger: loggerMock.Object);

            // Act
            testProcess.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "The BuildHost process is not responding. Process output:{newLine}{processLog}",
                    Environment.NewLine,
                    logMessages),
                Times.Once);
        }

        [Fact]
        public void LogProcessFailure_LogsError_WhenProcessExitedWithNonZeroExitCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var logMessages = "Error output";
            int exitCode = 123;
            var testProcess = new TestBuildHostProcess(exitCode: exitCode, hasExited: true, logMessages: logMessages, logger: loggerMock.Object);

            // Act
            testProcess.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "The BuildHost process exited with {errorCode}. Process output:{newLine}{processLog}",
                    exitCode,
                    Environment.NewLine,
                    logMessages),
                Times.Once);
        }

        [Fact]
        public void LogProcessFailure_DoesNotLog_WhenLoggerIsNull()
        {
            // Arrange
            var testProcess = new TestBuildHostProcess(exitCode: 1, hasExited: true, logMessages: "log", logger: null);

            // Act & Assert
            // Should not throw
            testProcess.LogProcessFailure();
        }
    }
}
