using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace Microsoft.CodeAnalysis.MSBuild.Tests
{
    public class BuildHostProcessManagerTests
    {
        [Fact]
        public void LogProcessFailure_LogsErrorWhenProcessFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new LoggerFactory().CreateLogger<BuildHostProcessManager>());

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void LogProcessFailure_LogsErrorWithCorrectMessageWhenProcessFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BuildHostProcessManager>>();
            var buildHostProcessManager = new BuildHostProcessManager(loggerFactory: new LoggerFactory().CreateLogger<BuildHostProcessManager>());

            // Act
            buildHostProcessManager.LogProcessFailure();

            // Assert
            loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "The BuildHost process is not responding. Process output:{newLine}{processLog}", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
