using Microsoft.CodeAnalysis.MSBuild;
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
        public void LogProcessFailure_LogsError_WhenProcessHasNotExited()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var process = new Process();
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = "run";
            process.Start();
            var buildHostProcessManager = new BuildHostProcessManager(null, null, loggerMock.Object);
            buildHostProcessManager._process = process;

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void LogProcessFailure_LogsError_WhenProcessHasExitedWithNonZeroExitCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var process = new Process();
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = "run";
            process.Start();
            process.Kill();
            var buildHostProcessManager = new BuildHostProcessManager(null, null, loggerMock.Object);
            buildHostProcessManager._process = process;

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void LogProcessFailure_DoesNotLogError_WhenLoggerIsNull()
        {
            // Arrange
            var process = new Process();
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = "run";
            process.Start();
            var buildHostProcessManager = new BuildHostProcessManager(null, null, null);
            buildHostProcessManager._process = process;

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            // No exception is thrown
        }
    }
}
