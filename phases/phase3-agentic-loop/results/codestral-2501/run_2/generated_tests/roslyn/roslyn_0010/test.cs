using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public void LogProcessFailure_LogsError_WhenProcessExitedWithNonZeroCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            var processManager = new BuildHostProcessManager(loggerFactory: Mock.Of<ILoggerFactory>());

            processMock.Setup(p => p.HasExited).Returns(true);
            processMock.Setup(p => p.ExitCode).Returns(1);

            var processLogMessages = new StringWriter();
            processLogMessages.Write("Some process log");

            // Set private fields using reflection
            var loggerField = typeof(BuildHostProcessManager).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var processField = typeof(BuildHostProcessManager).GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var processLogMessagesField = typeof(BuildHostProcessManager).GetField("_processLogMessages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            loggerField.SetValue(processManager, loggerMock.Object);
            processField.SetValue(processManager, processMock.Object);
            processLogMessagesField.SetValue(processManager, processLogMessages);

            // Act
            processManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The BuildHost process exited with 1. Process output:")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
