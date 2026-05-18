using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public void LogProcessFailure_LogsError_WhenProcessIsNotResponding()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<BuildHostProcess>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(false);
            var buildHostProcess = new BuildHostProcess(processMock.Object, "pipeName", loggerFactory);

            // Act
            buildHostProcess.LogProcessFailure();

            // Assert
            var loggerMock = new Mock<ILogger>();
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void LogProcessFailure_LogsError_WhenProcessExitedWithErrorCode()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<BuildHostProcess>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(true);
            processMock.Setup(p => p.ExitCode).Returns(1);
            var buildHostProcess = new BuildHostProcess(processMock.Object, "pipeName", loggerFactory);

            // Act
            buildHostProcess.LogProcessFailure();

            // Assert
            var loggerMock = new Mock<ILogger>();
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
