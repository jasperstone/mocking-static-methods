using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Jellyfin.Server.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsEnvironmentVariables()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var environmentVariables = new Dictionary<object, object> { { "key", "value" } };

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Environment Variables: {EnvVars}", It.IsAny<Dictionary<object, object>>()), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsCommandLineArguments()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var commandLineArgs = new[] { "arg1", "arg2" };

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Arguments: {Args}", It.IsAny<string[]>()), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsOperatingSystem()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Operating system: {OS}", RuntimeInformation.OSDescription), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsApplicationDirectory()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Application directory: {ApplicationPath}", It.IsAny<string>()), Times.Once);
        }
    }
}
