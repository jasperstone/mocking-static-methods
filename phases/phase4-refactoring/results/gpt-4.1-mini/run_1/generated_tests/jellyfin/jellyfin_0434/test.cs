using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Jellyfin.Server.Helpers;
using RealIApplicationPaths = MediaBrowser.Common.Configuration.IApplicationPaths;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        private class TestApplicationPaths : RealIApplicationPaths
        {
            public string ProgramDataPath => "/program/data";
            public string LogDirectoryPath => "/log/dir";
            public string ConfigurationDirectoryPath => "/config/dir";
            public string CachePath => "/cache/path";
            public string TempDirectory => "/temp/dir";
            public string WebPath => "/web/path";
            public string ProgramSystemPath => "/program/system/path";
        }

        [Fact]
        public void LogEnvironmentInfo_LogsApplicationDirectory()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new TestApplicationPaths();

            var loggedMessages = new List<string>();
            var loggedArgs = new List<object[]>();

            loggerMock.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Callback((LogLevel level, EventId eventId, object state, Exception ex, Delegate formatter) =>
            {
                var message = formatter.DynamicInvoke(state, ex) as string;
                loggedMessages.Add(message ?? "");
                var stateProperties = state.GetType().GetProperty("Values")?.GetValue(state) as IEnumerable<KeyValuePair<string, object>>;
                var args = stateProperties?.Select(kvp => kvp.Value).ToArray() ?? Array.Empty<object>();
                loggedArgs.Add(args);
            });

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            bool foundApplicationDirectoryLog = false;
            for (int i = 0; i < loggedMessages.Count; i++)
            {
                if (loggedMessages[i].StartsWith("Application directory:", StringComparison.OrdinalIgnoreCase) &&
                    loggedArgs[i].Contains(appPaths.ProgramSystemPath))
                {
                    foundApplicationDirectoryLog = true;
                    break;
                }
            }
            Assert.True(foundApplicationDirectoryLog, "Expected log message for Application directory was not found.");
        }
    }
}
