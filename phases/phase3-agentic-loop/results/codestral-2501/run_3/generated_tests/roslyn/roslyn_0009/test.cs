using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        private readonly Mock<ILogger<BuildHostProcessManager>> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly BuildHostProcessManager _processManager;

        public BuildHostProcessManagerTests()
        {
            _loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerFactoryMock.Setup(factory => factory.CreateLogger<BuildHostProcessManager>()).Returns(_loggerMock.Object);

            _processManager = new BuildHostProcessManager(
                globalMSBuildProperties: ImmutableDictionary<string, string>.Empty,
                binaryLogPathProvider: null,
                loggerFactory: _loggerFactoryMock.Object);
        }

        [Fact]
        public void LogProcessFailure_ProcessNotResponding_LogsError()
        {
            // Arrange
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(false);
            processMock.Setup(p => p.ExitCode).Returns(0);

            var processLogMessages = new StringWriter();
            processLogMessages.Write("Process log message");

            var processManagerType = typeof(BuildHostProcessManager);
            var processField = processManagerType.GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processField.SetValue(_processManager, processMock.Object);

            var processLogMessagesField = processManagerType.GetField("_processLogMessages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processLogMessagesField.SetValue(_processManager, processLogMessages);

            // Act
            _processManager.LogProcessFailure();

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    "The BuildHost process is not responding. Process output:{newLine}{processLog}",
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task ShutdownAsync_Exception_LogsError()
        {
            // Arrange
            var buildHostMock = new Mock<RemoteBuildHost>();
            buildHostMock.Setup(host => host.ShutdownAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test exception"));

            var processManagerType = typeof(BuildHostProcessManager);
            var buildHostField = processManagerType.GetField("BuildHost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            buildHostField.SetValue(_processManager, buildHostMock.Object);

            var processMock = new Mock<Process>();
            var processField = processManagerType.GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processField.SetValue(_processManager, processMock.Object);

            // Act
            await Assert.ThrowsAsync<Exception>(() => _processManager.ShutdownAsync(CancellationToken.None));

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Exception while shutting down the BuildHost process."),
                Times.Once);
        }
    }
}
