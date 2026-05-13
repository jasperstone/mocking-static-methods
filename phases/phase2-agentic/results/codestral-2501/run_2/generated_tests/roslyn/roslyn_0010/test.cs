using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public void LogProcessFailure_ProcessExitedWithNonZeroExitCode_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(true);
            processMock.Setup(p => p.ExitCode).Returns(1);
            var processLogMessages = new StringWriter();
            processLogMessages.Write("Process log message");

            var buildHostProcessManager = new BuildHostProcessManager(
                loggerFactory: Mock.Of<ILoggerFactory>(),
                binaryLogPathProvider: null,
                globalMSBuildProperties: null
            );

            var buildHostProcessManagerType = typeof(BuildHostProcessManager);
            var processField = buildHostProcessManagerType.GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processField.SetValue(buildHostProcessManager, processMock.Object);

            var loggerField = buildHostProcessManagerType.GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(buildHostProcessManager, loggerMock.Object);

            var processLogMessagesField = buildHostProcessManagerType.GetField("_processLogMessages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processLogMessagesField.SetValue(buildHostProcessManager, processLogMessages);

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The BuildHost process exited with 1. Process output:")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void LogProcessFailure_ProcessNotResponding_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(false);
            var processLogMessages = new StringWriter();
            processLogMessages.Write("Process log message");

            var buildHostProcessManager = new BuildHostProcessManager(
                loggerFactory: Mock.Of<ILoggerFactory>(),
                binaryLogPathProvider: null,
                globalMSBuildProperties: null
            );

            var buildHostProcessManagerType = typeof(BuildHostProcessManager);
            var processField = buildHostProcessManagerType.GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processField.SetValue(buildHostProcessManager, processMock.Object);

            var loggerField = buildHostProcessManagerType.GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(buildHostProcessManager, loggerMock.Object);

            var processLogMessagesField = buildHostProcessManagerType.GetField("_processLogMessages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processLogMessagesField.SetValue(buildHostProcessManager, processLogMessages);

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The BuildHost process is not responding. Process output:")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
