using System;
using System.Diagnostics;
using System.Threading;
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
            processMock.Setup(p => p.HasExited).Returns(true);
            processMock.Setup(p => p.ExitCode).Returns(1);

            var processLogMessages = new System.Text.StringBuilder();
            var buildHostProcessManager = new BuildHostProcessManager(
                loggerFactory: null,
                logger: loggerMock.Object,
                process: processMock.Object,
                processLogMessages: processLogMessages);

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("The BuildHost process exited with 1")),
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
