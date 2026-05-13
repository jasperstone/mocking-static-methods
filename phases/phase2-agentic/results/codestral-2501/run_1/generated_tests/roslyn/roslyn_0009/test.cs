using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Roslyn.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task LogError_WhenShutdownAsyncThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var buildHostMock = new Mock<RemoteBuildHost>();
            var processMock = new Mock<Process>();
            var processManager = new BuildHostProcessManager(loggerFactory: loggerMock.Object);

            processMock.Setup(p => p.HasExited).Returns(false);
            processMock.Setup(p => p.ExitCode).Returns(1);
            processMock.Setup(p => p.Kill());

            buildHostMock.Setup(bh => bh.ShutdownAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test exception"));

            // Act
            await processManager.ShutdownAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Exception while shutting down the BuildHost process."),
                Times.Once);

            loggerMock.Verify(
                logger => logger.LogError(
                    "The BuildHost process is not responding. Process output:{newLine}{processLog}",
                    Environment.NewLine,
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void LogError_WhenProcessHasExitedWithNonZeroExitCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var processMock = new Mock<Process>();
            var processManager = new BuildHostProcessManager(loggerFactory: loggerMock.Object);

            processMock.Setup(p => p.HasExited).Returns(true);
            processMock.Setup(p => p.ExitCode).Returns(1);

            // Act
            processManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    "The BuildHost process exited with {errorCode}. Process output:{newLine}{processLog}",
                    1,
                    Environment.NewLine,
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void LogError_WhenProcessIsNotResponding()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var processMock = new Mock<Process>();
            var processManager = new BuildHostProcessManager(loggerFactory: loggerMock.Object);

            processMock.Setup(p => p.HasExited).Returns(false);

            // Act
            processManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    "The BuildHost process is not responding. Process output:{newLine}{processLog}",
                    Environment.NewLine,
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
