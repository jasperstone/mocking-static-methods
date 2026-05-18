using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests
{
    public class BuildHostProcessTests
    {
        [Fact]
        public void LogProcessFailure_WhenProcessNotExited_LogsCorrectError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(false);

            var processLogMessages = new StringBuilder("Test process output\nLine 2");

            var buildHostProcess = new BuildHostProcessPrivate(processMock.Object, "test-pipe", null)
            {
                _logger = loggerMock.Object,
                _process = processMock.Object,
                _processLogMessages = processLogMessages
            };

            // Act
            buildHostProcess.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, t) => 
                        t!.ToString() == "The BuildHost process is not responding. Process output:{newLine}{processLog}"
                        && state.ToString()!.Contains("Test process output")
                        && state.ToString()!.Contains("Line 2")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogProcessFailure_WhenProcessExitedNonZero_LogsCorrectError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(true);
            processMock.Setup(p => p.ExitCode).Returns(123);

            var processLogMessages = new StringBuilder("Test process output");

            var buildHostProcess = new BuildHostProcessPrivate(processMock.Object, "test-pipe", null)
            {
                _logger = loggerMock.Object,
                _process = processMock.Object,
                _processLogMessages = processLogMessages
            };

            // Act
            buildHostProcess.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, t) => 
                        t!.ToString() == "The BuildHost process exited with {errorCode}. Process output:{newLine}{processLog}"
                        && state.ToString()!.Contains("123")
                        && state.ToString()!.Contains("Test process output")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogProcessFailure_WhenNoLogger_DoesNothing()
        {
            // Arrange
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(false);

            var processLogMessages = new StringBuilder();

            var buildHostProcess = new BuildHostProcessPrivate(processMock.Object, "test-pipe", null)
            {
                _process = processMock.Object,
                _processLogMessages = processLogMessages
            };

            // Act
            buildHostProcess.LogProcessFailure();

            // Assert - no exception thrown
            Assert.True(true);
        }
    }

    // Partial class to access internal LogProcessFailure method for testing
    internal partial class BuildHostProcessPrivate : Microsoft.CodeAnalysis.MSBuild.BuildHostProcess
    {
        public BuildHostProcessPrivate(Process process, string pipeName, Microsoft.Extensions.Logging.ILoggerFactory? loggerFactory)
            : base(process, pipeName, loggerFactory)
        {
        }

        public new void LogProcessFailure() => base.LogProcessFailure();
    }
}
