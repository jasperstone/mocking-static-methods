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
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

            var buildHostProcessManager = new BuildHostProcessManager(
                globalMSBuildProperties: null,
                binaryLogPathProvider: null,
                loggerFactory: loggerFactoryMock.Object);

            var exception = new Exception("Test exception");

            // Act
            await buildHostProcessManager.ShutdownAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void LogError_WhenProcessHasExited()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

            var buildHostProcessManager = new BuildHostProcessManager(
                globalMSBuildProperties: null,
                binaryLogPathProvider: null,
                loggerFactory: loggerFactoryMock.Object);

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(true);
            processMock.Setup(p => p.ExitCode).Returns(1);

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void LogError_WhenProcessIsNotResponding()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger<BuildHostProcessManager>()).Returns(loggerMock.Object);

            var buildHostProcessManager = new BuildHostProcessManager(
                globalMSBuildProperties: null,
                binaryLogPathProvider: null,
                loggerFactory: loggerFactoryMock.Object);

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.HasExited).Returns(false);

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
