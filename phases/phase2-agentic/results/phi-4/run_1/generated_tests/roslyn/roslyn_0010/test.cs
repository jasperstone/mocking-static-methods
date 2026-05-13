using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Workspaces.Tests
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
            var processLogMessages = new StringBuilder();
            processLogMessages.AppendLine("Sample log message");

            var manager = new BuildHostProcessManager(loggerMock.Object, processMock.Object, processLogMessages);

            // Act
            manager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("The BuildHost process exited with 1.")),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }

        [Fact]
        public void LogProcessFailure_LogsError_WhenProcessIsNotResponding()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(false);
            var processLogMessages = new StringBuilder();
            processLogMessages.AppendLine("Sample log message");

            var manager = new BuildHostProcessManager(loggerMock.Object, processMock.Object, processLogMessages);

            // Act
            manager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("The BuildHost process is not responding.")),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }
    }
}
