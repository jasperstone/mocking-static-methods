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
        public void LogProcessFailure_LogsError_WhenProcessNotExited()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(false);
            var processLogMessages = new StringBuilder();
            var buildHostProcessManager = new BuildHostProcessManager(null);

            // Use reflection to set private fields
            var loggerField = typeof(BuildHostProcessManager).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance);
            var processField = typeof(BuildHostProcessManager).GetField("_process", BindingFlags.NonPublic | BindingFlags.Instance);
            var processLogMessagesField = typeof(BuildHostProcessManager).GetField("_processLogMessages", BindingFlags.NonPublic | BindingFlags.Instance);

            loggerField.SetValue(buildHostProcessManager, loggerMock.Object);
            processField.SetValue(buildHostProcessManager, processMock.Object);
            processLogMessagesField.SetValue(buildHostProcessManager, processLogMessages);

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("The BuildHost process is not responding")),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void LogProcessFailure_LogsError_WhenProcessExitedWithNonZeroCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(true);
            processMock.Setup(p => p.ExitCode).Returns(1);
            var processLogMessages = new StringBuilder();
            var buildHostProcessManager = new BuildHostProcessManager(null);

            // Use reflection to set private fields
            var loggerField = typeof(BuildHostProcessManager).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance);
            var processField = typeof(BuildHostProcessManager).GetField("_process", BindingFlags.NonPublic | BindingFlags.Instance);
            var processLogMessagesField = typeof(BuildHostProcessManager).GetField("_processLogMessages", BindingFlags.NonPublic | BindingFlags.Instance);

            loggerField.SetValue(buildHostProcessManager, loggerMock.Object);
            processField.SetValue(buildHostProcessManager, processMock.Object);
            processLogMessagesField.SetValue(buildHostProcessManager, processLogMessages);

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("The BuildHost process exited with 1")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
