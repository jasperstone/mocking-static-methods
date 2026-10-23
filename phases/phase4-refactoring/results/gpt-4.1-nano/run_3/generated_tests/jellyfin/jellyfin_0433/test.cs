using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Jellyfin.Server.Helpers;

namespace Jellyfin.Tests
{
    public class StartupHelpersTests
    {
        private class DummyAppPaths : IApplicationPaths
        {
            public string ProgramDataPath { get; set; } = "ProgramData";
            public string LogDirectoryPath { get; set; } = "LogDir";
            public string ConfigurationDirectoryPath { get; set; } = "ConfigDir";
            public string CachePath { get; set; } = "Cache";
            public string WebPath { get; set; } = "Web";
            public string TempDirectory { get; set; } = "Temp";
            public string ProgramSystemPath { get; set; } = "AppPath";
        }

        [Fact]
        public void LogEnvironmentInfo_ShouldLogEnvironmentVariablesAndSystemInfo()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var appPaths = new DummyAppPaths();

            // Set environment variables
            Environment.SetEnvironmentVariable("JELLYFIN_TEST_VAR", "value");
            Environment.SetEnvironmentVariable("DOTNET_TEST_VAR2", "value2");
            Environment.SetEnvironmentVariable("UNRELATED_VAR", "should_not_appear");
            Environment.SetEnvironmentVariable("JELLYFIN_OTHER", "ignored");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, appPaths);

            // Assert
            // Verify that LogInformation was called with the expected message containing the environment variables dictionary
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Environment Variables:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Verify that the environment variables dictionary contains the relevant variables
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Environment Variables:") &&
                        v.ToString().Contains("JELLYFIN_TEST_VAR") &&
                        v.ToString().Contains("DOTNET_TEST_VAR2")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
