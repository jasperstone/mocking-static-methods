using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.UnitTests
{
    public class LoggerExtensionsTests
    {
        private class TestLoggerCaller
        {
            private readonly ILogger? _logger;
            private readonly bool _hasExited;
            private readonly int _exitCode;
            private readonly StringBuilder _processLogMessages;

            public TestLoggerCaller(ILogger? logger, bool hasExited, int exitCode, string processLog)
            {
                _logger = logger;
                _hasExited = hasExited;
                _exitCode = exitCode;
                _processLogMessages = new StringBuilder(processLog);
            }

            public void LogProcessFailure()
            {
                if (_logger == null)
                    return;

                string processLog;
                lock (_processLogMessages)
                    processLog = _processLogMessages.ToString();

                if (!_hasExited)
                    _logger.LogError("The BuildHost process is not responding. Process output:{newLine}{processLog}", Environment.NewLine, processLog);
                else if (_exitCode != 0)
                    _logger.LogError("The BuildHost process exited with {errorCode}. Process output:{newLine}{processLog}", _exitCode, Environment.NewLine, processLog);
            }
        }

        [Fact]
        public void LogProcessFailure_NoLogger_DoesNotThrow()
        {
            var caller = new TestLoggerCaller(null, hasExited: false, exitCode: 0, processLog: "");
            caller.LogProcessFailure();
        }

        [Fact]
        public void LogProcessFailure_ProcessNotExited_LogsNotResponding()
        {
            var loggerMock = new Mock<ILogger>();
            var logContent = "some log content";

            var caller = new TestLoggerCaller(loggerMock.Object, hasExited: false, exitCode: 0, processLog: logContent);
            caller.LogProcessFailure();

            loggerMock.Verify(l => l.LogError(
                "The BuildHost process is not responding. Process output:{newLine}{processLog}",
                Environment.NewLine,
                logContent), Times.Once);
        }

        [Fact]
        public void LogProcessFailure_ProcessExitedWithError_LogsExitCode()
        {
            var loggerMock = new Mock<ILogger>();
            var logContent = "error log content";
            int exitCode = 123;

            var caller = new TestLoggerCaller(loggerMock.Object, hasExited: true, exitCode: exitCode, processLog: logContent);
            caller.LogProcessFailure();

            loggerMock.Verify(l => l.LogError(
                "The BuildHost process exited with {errorCode}. Process output:{newLine}{processLog}",
                exitCode,
                Environment.NewLine,
                logContent), Times.Once);
        }
    }
}
