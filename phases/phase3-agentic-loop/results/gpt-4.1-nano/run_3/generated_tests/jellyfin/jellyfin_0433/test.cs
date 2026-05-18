using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Jellyfin.Server.Helpers;
using System.IO;

namespace Jellyfin.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsExpectedInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(p => p.ProgramDataPath).Returns("DataPath");
            mockAppPaths.SetupGet(p => p.LogDirectoryPath).Returns("LogPath");
            mockAppPaths.SetupGet(p => p.ConfigurationDirectoryPath).Returns("ConfigPath");
            mockAppPaths.SetupGet(p => p.CachePath).Returns("CachePath");
            mockAppPaths.SetupGet(p => p.TempDirectory).Returns("TempPath");
            mockAppPaths.SetupGet(p => p.WebPath).Returns("WebPath");
            mockAppPaths.SetupGet(p => p.ProgramSystemPath).Returns("AppPath");

            // Save original environment variables
            var originalEnvVars = Environment.GetEnvironmentVariables();

            // Set environment variables for test
            Environment.SetEnvironmentVariable("JELLYFIN_TEST_VAR", "value");
            Environment.SetEnvironmentVariable("DOTNET_TEST_VAR", "value");
            Environment.SetEnvironmentVariable("UNRELATED_VAR", "value");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Reset environment variables
            foreach (var key in originalEnvVars.Keys)
            {
                Environment.SetEnvironmentVariable(key.ToString(), originalEnvVars[key]?.ToString());
            }

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Environment Variables:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Arguments:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Operating system:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
