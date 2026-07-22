using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Jellyfin.Server.Helpers;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_ShouldLogEnvironmentVariablesAndSystemInfo()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            var envVars = new Dictionary<string, string>
            {
                { "JELLYFIN_TEST_VAR", "value1" },
                { "DOTNET_TEST_VAR", "value2" },
                { "OTHER_VAR", "value3" }
            };

            // Set environment variables
            foreach (var kvp in envVars)
            {
                Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
            }

            // Setup mock AppPaths
            mockAppPaths.SetupGet(p => p.ProgramDataPath).Returns("ProgramDataPath");
            mockAppPaths.SetupGet(p => p.LogDirectoryPath).Returns("LogDir");
            mockAppPaths.SetupGet(p => p.ConfigurationDirectoryPath).Returns("ConfigDir");
            mockAppPaths.SetupGet(p => p.CachePath).Returns("CachePath");
            mockAppPaths.SetupGet(p => p.TempDirectory).Returns("TempDir");
            mockAppPaths.SetupGet(p => p.WebPath).Returns("WebPath");
            mockAppPaths.SetupGet(p => p.ProgramSystemPath).Returns("AppPath");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Verify that LogInformation was called with expected messages
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

            // Check that relevant environment variables are included
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Environment Variables: {EnvVars}")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
