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
        public void LogEnvironmentInfo_LogsExpectedInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(p => p.ProgramDataPath).Returns("/programData");
            mockAppPaths.SetupGet(p => p.LogDirectoryPath).Returns("/logs");
            mockAppPaths.SetupGet(p => p.ConfigurationDirectoryPath).Returns("/config");
            mockAppPaths.SetupGet(p => p.CachePath).Returns("/cache");
            mockAppPaths.SetupGet(p => p.TempDirectory).Returns("/temp");
            mockAppPaths.SetupGet(p => p.WebPath).Returns("/web");
            mockAppPaths.SetupGet(p => p.ProgramSystemPath).Returns("/app");

            // Set environment variables
            Environment.SetEnvironmentVariable("JELLYFIN_TEST", "value");
            Environment.SetEnvironmentVariable("DOTNET_TEST", "value");
            Environment.SetEnvironmentVariable("OTHER_VAR", "value");
            Environment.SetEnvironmentVariable("JELLYFIN_DATA_DIR", "/data");
            Environment.SetEnvironmentVariable("JELLYFIN_CONFIG_DIR", "/config");
            Environment.SetEnvironmentVariable("JELLYFIN_CACHE_DIR", "/cache");
            Environment.SetEnvironmentVariable("JELLYFIN_WEB_DIR", "/web");
            Environment.SetEnvironmentVariable("JELLYFIN_LOG_DIR", "/log");
            Environment.SetEnvironmentVariable("XDG_CACHE_HOME", "/xdg/cache");
            Environment.SetEnvironmentVariable("HOME", "/home/user");

            // Capture logs
            var logs = new List<(LogLevel, string, object[])>();
            mockLogger.Setup(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It, string, object[]>(), It.IsAny<Exception>(), (Func<It, Exception, string>)It.IsAny<object>()))
                .Callback<LogLevel, EventId, object, Exception, Func<It, Exception, string>>((level, eventId, state, exception, formatter) =>
                {
                    var message = formatter(state, exception);
                    logs.Add((level, message, state.GetType().GetProperties().Select(p => p.GetValue(state)).ToArray()));
                });

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert
            Assert.Contains(logs, log => log.Item2.Contains("Environment Variables"));
            Assert.Contains(logs, log => log.Item2.Contains("Arguments"));
            Assert.Contains(logs, log => log.Item2.Contains("Operating system"));
            Assert.Contains(logs, log => log.Item2.Contains("Architecture"));
            Assert.Contains(logs, log => log.Item2.Contains("64-Bit Process"));
            Assert.Contains(logs, log => log.Item2.Contains("User Interactive"));
            Assert.Contains(logs, log => log.Item2.Contains("Processor count"));
            Assert.Contains(logs, log => log.Item2.Contains("/programData"));
            Assert.Contains(logs, log => log.Item2.Contains("/logs"));
            Assert.Contains(logs, log => log.Item2.Contains("/config"));
            Assert.Contains(logs, log => log.Item2.Contains("/cache"));
            Assert.Contains(logs, log => log.Item2.Contains("/temp"));
            Assert.Contains(logs, log => log.Item2.Contains("/web"));
            Assert.Contains(logs, log => log.Item2.Contains("/app"));
        }
    }
}
