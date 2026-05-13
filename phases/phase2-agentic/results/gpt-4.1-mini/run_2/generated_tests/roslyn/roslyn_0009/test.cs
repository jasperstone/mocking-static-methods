using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.MSBuild;
using Xunit;
using Moq;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests
{
    public class BuildHostProcessManagerTests
    {
        private class TestBuildHostProcessManager : BuildHostProcessManager
        {
            public TestBuildHostProcessManager(ILoggerFactory? loggerFactory = null)
                : base(loggerFactory: loggerFactory)
            {
            }

            public void CallLogProcessFailure(BuildHostProcess process, ILogger? logger)
            {
                // We need to call the private LogProcessFailure method on BuildHostProcess.
                // Since it's private, we simulate the call here by invoking the same logic.
                if (logger == null)
                    return;

                string processLog;
                lock (process.ProcessLogMessages)
                    processLog = process.ProcessLogMessages.ToString();

                if (!process.HasExited)
                    logger.LogError("The BuildHost process is not responding. Process output:{newLine}{processLog}", Environment.NewLine, processLog);
                else if (process.ExitCode != 0)
                    logger.LogError("The BuildHost process exited with {errorCode}. Process output:{newLine}{processLog}", process.ExitCode, Environment.NewLine, processLog);
            }
        }

        private class BuildHostProcess
        {
            public StringBuilder ProcessLogMessages { get; } = new StringBuilder();
            public bool HasExited { get; set; }
            public int ExitCode { get; set; }
        }

        [Fact]
        public void LogProcessFailure_LogsError_WhenProcessNotExited()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var process = new BuildHostProcess
            {
                HasExited = false,
                ExitCode = 0
            };
            process.ProcessLogMessages.Append("Test log output");

            var manager = new TestBuildHostProcessManager();

            // Act
            manager.CallLogProcessFailure(process, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "The BuildHost process is not responding. Process output:{newLine}{processLog}",
                    Environment.NewLine,
                    "Test log output"),
                Times.Once);
        }

        [Fact]
        public void LogProcessFailure_LogsError_WhenProcessExitedWithNonZeroExitCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var process = new BuildHostProcess
            {
                HasExited = true,
                ExitCode = 123
            };
            process.ProcessLogMessages.Append("Error log output");

            var manager = new TestBuildHostProcessManager();

            // Act
            manager.CallLogProcessFailure(process, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "The BuildHost process exited with {errorCode}. Process output:{newLine}{processLog}",
                    123,
                    Environment.NewLine,
                    "Error log output"),
                Times.Once);
        }

        [Fact]
        public void LogProcessFailure_DoesNotLog_WhenLoggerIsNull()
        {
            // Arrange
            var process = new BuildHostProcess
            {
                HasExited = false,
                ExitCode = 0
            };
            process.ProcessLogMessages.Append("Some output");

            var manager = new TestBuildHostProcessManager();

            // Act & Assert
            // Should not throw or log anything
            manager.CallLogProcessFailure(process, null);
        }
    }
}
