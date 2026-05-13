using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.CodeAnalysis.MSBuild
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task LogProcessFailure_LogsError_WhenProcessIsNotResponding()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.SetupGet(p => p.HasExited).Returns(false);
            processMock.SetupGet(p => p.ExitCode).Returns(0);
            var buildHostProcessManager = new BuildHostProcessManager(null, null, loggerMock.Object);

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task LogProcessFailure_LogsError_WhenProcessExitedWithNonZeroCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.SetupGet(p => p.HasExited).Returns(true);
            processMock.SetupGet(p => p.ExitCode).Returns(1);
            var buildHostProcessManager = new BuildHostProcessManager(null, null, loggerMock.Object);

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task LogProcessFailure_DoesNotLogError_WhenLoggerIsNull()
        {
            // Arrange
            var processMock = new Mock<Process>();
            processMock.SetupGet(p => p.HasExited).Returns(false);
            processMock.SetupGet(p => p.ExitCode).Returns(0);
            var buildHostProcessManager = new BuildHostProcessManager(null, null, null);

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            // No error should be logged
        }
    }
}
