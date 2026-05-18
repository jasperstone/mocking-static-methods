using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
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
                loggerFactory: new LoggerFactory().AddProvider(new MockLoggerProvider(loggerMock.Object)),
                globalMSBuildProperties: null,
                binaryLogPathProvider: null);

            // Use reflection to set private fields
            var processField = typeof(BuildHostProcessManager).GetField("_process", BindingFlags.NonPublic | BindingFlags.Instance);
            var processLogMessagesField = typeof(BuildHostProcessManager).GetField("_processLogMessages", BindingFlags.NonPublic | BindingFlags.Instance);

            processField.SetValue(buildHostProcessManager, processMock.Object);
            processLogMessagesField.SetValue(buildHostProcessManager, processLogMessages);

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("The BuildHost process exited with 1.")),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>()
                ),
                Times.Once);
        }

        [Fact]
        public void LogProcessFailure_LogsError_WhenProcessIsNotResponding()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(false);
            var processLogMessages = new StringBuilder("Sample log messages");

            var buildHostProcessManager = new BuildHostProcessManager(
                loggerFactory: new LoggerFactory().AddProvider(new MockLoggerProvider(loggerMock.Object)),
                globalMSBuildProperties: null,
                binaryLogPathProvider: null);

            // Use reflection to set private fields
            var processField = typeof(BuildHostProcessManager).GetField("_process", BindingFlags.NonPublic | BindingFlags.Instance);
            var processLogMessagesField = typeof(BuildHostProcessManager).GetField("_processLogMessages", BindingFlags.NonPublic | BindingFlags.Instance);

            processField.SetValue(buildHostProcessManager, processMock.Object);
            processLogMessagesField.SetValue(buildHostProcessManager, processLogMessages);

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("The BuildHost process is not responding.")),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>()
                ),
                Times.Once);
        }
    }

    public class MockLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public MockLoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { }
    }
}
