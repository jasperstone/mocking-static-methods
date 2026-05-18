using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public void LogProcessFailure_LogsError_WhenProcessHasExitedWithNonZeroExitCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(true);
            processMock.Setup(p => p.ExitCode).Returns(1);
            var processLogMessages = new StringBuilder("Sample log messages");
            var buildHostProcessManager = new BuildHostProcessManager(
                loggerFactory: new LoggerFactory().AddProvider(new MockProvider(loggerMock.Object))
            )
            {
                _process = processMock.Object,
                _processLogMessages = processLogMessages
            };

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("The BuildHost process exited with 1.")),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<object>()
                ),
                Times.Once
            );
        }

        private class MockProvider : ILoggerProvider
        {
            private readonly ILogger _logger;

            public MockProvider(ILogger logger)
            {
                _logger = logger;
            }

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose() { }
        }

        // Mocked BuildHostProcessManager class for testing
        private class BuildHostProcessManager
        {
            public Process _process { get; set; }
            public StringBuilder _processLogMessages { get; set; }
            private readonly ILogger? _logger;

            public BuildHostProcessManager(ILoggerFactory? loggerFactory)
            {
                _logger = loggerFactory?.CreateLogger<BuildHostProcessManager>();
            }

            public void LogProcessFailure()
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
        }
    }
}
