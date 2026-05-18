using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public void LogProcessFailure_LogsErrorWithNonZeroExitCode()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var processLogMessages = new List<string> { "Test process output" };
            var mockProcess = new Mock<Process>();
            mockProcess.Setup(p => p.HasExited).Returns(true);
            mockProcess.Setup(p => p.ExitCode).Returns(1);

            var manager = new TestableBuildHostProcessManager(mockLogger.Object, mockProcess.Object, processLogMessages);

            // Act
            manager.CallLogProcessFailure();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private class TestableBuildHostProcessManager
        {
            private readonly ILogger _logger;
            private readonly Process _process;
            private readonly List<string> _processLogMessages;

            public TestableBuildHostProcessManager(ILogger logger, Process process, List<string> processLogMessages)
            {
                _logger = logger;
                _process = process;
                _processLogMessages = processLogMessages;
            }

            public void CallLogProcessFailure()
            {
                if (_logger == null)
                    return;

                string processLog;
                lock (_processLogMessages)
                    processLog = string.Join(Environment.NewLine, _processLogMessages);

                if (!_process.HasExited)
                {
                    _logger.LogError("The BuildHost process is not responding. Process output:{newLine}{processLog}", Environment.NewLine, processLog);
                }
                else if (_process.ExitCode != 0)
                {
                    _logger.LogError("The BuildHost process exited with {errorCode}. Process output:{newLine}{processLog}", _process.ExitCode, Environment.NewLine, processLog);
                }
            }
        }
    }
}
