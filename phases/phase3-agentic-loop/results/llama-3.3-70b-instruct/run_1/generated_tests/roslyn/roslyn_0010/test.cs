using Xunit;
using Moq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public async Task LogError_Called_When_Process_Exits_With_NonZero_ExitCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new LoggerFactory().CreateLogger<BuildHostProcessManager>());
            buildHostProcessManager._logger = loggerMock.Object;
            buildHostProcessManager._process = new Process();
            buildHostProcessManager._process.StartInfo.FileName = "dotnet";
            buildHostProcessManager._process.Start();
            buildHostProcessManager._process.Kill();
            buildHostProcessManager._process.WaitForExit();
            buildHostProcessManager._process.ExitCode = 1;

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task LogError_Called_When_Process_Is_Not_Responding()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new LoggerFactory().CreateLogger<BuildHostProcessManager>());
            buildHostProcessManager._logger = loggerMock.Object;
            buildHostProcessManager._process = new Process();
            buildHostProcessManager._process.StartInfo.FileName = "dotnet";
            buildHostProcessManager._process.Start();

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
