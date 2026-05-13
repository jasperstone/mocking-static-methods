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
        [Fact]
        public void LogProcessFailure_WhenProcessNotResponding_LogsCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(false);
            var processLogMessages = new StringBuilder();
            var manager = new BuildHostProcessManager()
            {
                _logger = loggerMock.Object,
                _process = processMock.Object,
                _processLogMessages = processLogMessages
            };

            // Act
            manager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("The BuildHost process is not responding")),
                    It.IsAny<Exception>(),
                    It.Is<string>(s => s.Contains("Process output:")),
                    It.IsAny<string>()
                ),
                Times.Once);
        }

        [Fact]
        public void LogProcessFailure_WhenProcessExitedWithNonZeroCode_LogsCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(true);
            processMock.Setup(p => p.ExitCode).Returns(1);
            var processLogMessages = new StringBuilder();
            var manager = new BuildHostProcessManager()
            {
                _logger = loggerMock.Object,
                _process = processMock.Object,
                _processLogMessages = processLogMessages
            };

            // Act
            manager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("The BuildHost process exited with 1")),
                    It.IsAny<Exception>(),
                    It.Is<string>(s => s.Contains("Process output:")),
                    It.IsAny<string>()
                ),
                Times.Once);
        }

        [Fact]
        public void LogProcessFailure_WhenLoggerIsNull_DoesNothing()
        {
            // Arrange
            var processMock = new Mock<Process>();
            var processLogMessages = new StringBuilder();
            var manager = new BuildHostProcessManager()
            {
                _logger = null,
                _process = processMock.Object,
                _processLogMessages = processLogMessages
            };

            // Act
            manager.LogProcessFailure();

            // Assert
            // No verification needed as nothing should happen
        }
    }
}
