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
        public async Task LogError_WhenShutdownAsyncThrowsException_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var processMock = new Mock<Process>();
            var buildHostMock = new Mock<RemoteBuildHost>();
            var buildHostProcessMock = new Mock<BuildHostProcess>(processMock.Object, "pipeName", null);
            buildHostProcessMock.Setup(b => b.BuildHost).Returns(buildHostMock.Object);
            buildHostProcessMock.Setup(b => b.ShutdownAsync(CancellationToken.None)).Throws(new Exception("Test exception"));

            var manager = new BuildHostProcessManager(loggerFactory: loggerMock.Object);

            // Act
            await manager.ShutdownAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Exception while shutting down the BuildHost process."),
                Times.Once);
        }

        [Fact]
        public void LogProcessFailure_WhenProcessHasNotExited_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(false);
            processMock.Setup(p => p.Kill()).Verifiable();

            var manager = new BuildHostProcessManager(loggerFactory: loggerMock.Object);

            // Act
            manager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "The BuildHost process is not responding. Process output:{newLine}{processLog}",
                    Environment.NewLine,
                    It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void LogProcessFailure_WhenProcessExitedWithNonZeroCode_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(true);
            processMock.Setup(p => p.ExitCode).Returns(1);
            processMock.Setup(p => p.Kill()).Verifiable();

            var manager = new BuildHostProcessManager(loggerFactory: loggerMock.Object);

            // Act
            manager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "The BuildHost process exited with {errorCode}. Process output:{newLine}{processLog}",
                    1,
                    Environment.NewLine,
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
