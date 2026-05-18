using System;
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
        [Fact]
        public void LogProcessFailure_LogsError_WhenProcessHasExitedWithNonZeroExitCode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var processMock = new Mock<Process>();
            var processLogMessages = new StringWriter();

            processMock.Setup(p => p.HasExited).Returns(true);
            processMock.Setup(p => p.ExitCode).Returns(1);

            var buildHostProcessManager = new BuildHostProcessManager(
                loggerFactory: Mock.Of<ILoggerFactory>(),
                binaryLogPathProvider: null,
                globalMSBuildProperties: null
            );

            var privateObject = new PrivateObject(buildHostProcessManager);
            privateObject.SetFieldOrProperty("_logger", loggerMock.Object);
            privateObject.SetFieldOrProperty("_process", processMock.Object);
            privateObject.SetFieldOrProperty("_processLogMessages", processLogMessages);

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
    }
}
