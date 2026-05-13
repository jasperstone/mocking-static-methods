using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task LogProcessFailure_LogsError_WhenProcessHasExitedWithNonZeroExitCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.SetupGet(p => p.HasExited).Returns(true);
            processMock.SetupGet(p => p.ExitCode).Returns(1);
            var processLogMessages = new StringBuilder();
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new LoggerFactory().CreateLogger<BuildHostProcessManager>());

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task LogProcessFailure_LogsError_WhenProcessHasNotExited()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.SetupGet(p => p.HasExited).Returns(false);
            var processLogMessages = new StringBuilder();
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new LoggerFactory().CreateLogger<BuildHostProcessManager>());

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task LogProcessFailure_DoesNotLogError_WhenLoggerIsNull()
        {
            // Arrange
            var processMock = new Mock<Process>();
            processMock.SetupGet(p => p.HasExited).Returns(true);
            processMock.SetupGet(p => p.ExitCode).Returns(1);
            var processLogMessages = new StringBuilder();
            var buildHostProcessManager = new BuildHostProcessManager();

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            // No exception is thrown
        }
    }
}
