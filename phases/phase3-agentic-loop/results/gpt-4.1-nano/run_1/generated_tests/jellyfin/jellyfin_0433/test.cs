using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Jellyfin.Server.Helpers;

namespace Jellyfin.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_ShouldLogEnvironmentVariablesAndSystemInfo()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var appPaths = new DummyApplicationPaths();

            // Setup Environment variables
            Environment.SetEnvironmentVariable("JELLYFIN_TEST_VAR", "value");
            Environment.SetEnvironmentVariable("OTHER_VAR", "othervalue");
            Environment.SetEnvironmentVariable("JELLYFIN_ANOTHER", "anothervalue");
            Environment.SetEnvironmentVariable("XDG_CACHE_HOME", "/tmp/cache");
            Environment.SetEnvironmentVariable("HOME", "/home/testuser");
            Environment.SetEnvironmentVariable("JELLYFIN_DATA_DIR", "/custom/data");
            Environment.SetEnvironmentVariable("JELLYFIN_CONFIG_DIR", "/custom/config");
            Environment.SetEnvironmentVariable("JELLYFIN_CACHE_DIR", "/custom/cache");
            Environment.SetEnvironmentVariable("JELLYFIN_WEB_DIR", "/custom/web");
            Environment.SetEnvironmentVariable("JELLYFIN_LOG_DIR", "/custom/log");
            Environment.SetEnvironmentVariable("JELLYFIN_PROGRAM_PATH", "/program/path");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, appPaths);

            // Assert
            // Verify that LogInformation was called with expected messages
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Environment Variables:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Arguments:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Operating system:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Check that relevant environment variables are included
            var envVarsArg = CaptureLogArgument(mockLogger, "Environment Variables:");
            Assert.Contains("JELLYFIN_TEST_VAR", envVarsArg);
            Assert.Contains("JELLYFIN_ANOTHER", envVarsArg);
            Assert.DoesNotContain("OTHER_VAR", envVarsArg); // because it doesn't start with prefix

            // Cleanup environment variables
            Environment.SetEnvironmentVariable("JELLYFIN_TEST_VAR", null);
            Environment.SetEnvironmentVariable("OTHER_VAR", null);
            Environment.SetEnvironmentVariable("JELLYFIN_ANOTHER", null);
            Environment.SetEnvironmentVariable("XDG_CACHE_HOME", null);
            Environment.SetEnvironmentVariable("HOME", null);
            Environment.SetEnvironmentVariable("JELLYFIN_DATA_DIR", null);
            Environment.SetEnvironmentVariable("JELLYFIN_CONFIG_DIR", null);
            Environment.SetEnvironmentVariable("JELLYFIN_CACHE_DIR", null);
            Environment.SetEnvironmentVariable("JELLYFIN_WEB_DIR", null);
            Environment.SetEnvironmentVariable("JELLYFIN_LOG_DIR", null);
        }

        private string CaptureLogArgument(Mock<ILogger> mockLogger, string messageContains)
        {
            string captured = null;
            mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => {
                    var str = v.ToString();
                    if (str.Contains(messageContains))
                    {
                        captured = str;
                        return true;
                    }
                    return false;
                }),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Verifiable();
            return captured;
        }

        private class DummyApplicationPaths : IApplicationPaths
        {
            public string ProgramDataPath => "/program/data";
            public string LogDirectoryPath => "/log/directory";
            public string ConfigurationDirectoryPath => "/config/directory";
            public string CachePath => "/cache/path";
            public string WebPath => "/web/path";
            public string ProgramSystemPath => "/program/system/path";
        }
    }
}
