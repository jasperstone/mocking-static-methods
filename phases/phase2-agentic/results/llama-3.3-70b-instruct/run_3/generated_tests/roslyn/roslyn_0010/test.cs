using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
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
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new LoggerFactory().CreateLogger<BuildHostProcessManager>());

            // Act
            await buildHostProcessManager.LogProcessFailureAsync(processMock.Object, loggerMock.Object).ConfigureAwait(false);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "The BuildHost process exited with {errorCode}. Process output:{newLine}{processLog}", 1, Environment.NewLine, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task LogProcessFailure_LogsError_WhenProcessIsNotResponding()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.SetupGet(p => p.HasExited).Returns(false);
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new LoggerFactory().CreateLogger<BuildHostProcessManager>());

            // Act
            await buildHostProcessManager.LogProcessFailureAsync(processMock.Object, loggerMock.Object).ConfigureAwait(false);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "The BuildHost process is not responding. Process output:{newLine}{processLog}", Environment.NewLine, It.IsAny<string>()), Times.Once);
        }

        private async Task LogProcessFailureAsync(Process process, ILogger logger)
        {
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new LoggerFactory().CreateLogger<BuildHostProcessManager>());
            await buildHostProcessManager.LogProcessFailureAsync(process, logger).ConfigureAwait(false);
        }
    }

    internal static class BuildHostProcessManagerExtensions
    {
        public static async Task LogProcessFailureAsync(this BuildHostProcessManager buildHostProcessManager, Process process, ILogger logger)
        {
            buildHostProcessManager._logger = logger;
            buildHostProcessManager._process = process;
            buildHostProcessManager.LogProcessFailure();
        }
    }
}
