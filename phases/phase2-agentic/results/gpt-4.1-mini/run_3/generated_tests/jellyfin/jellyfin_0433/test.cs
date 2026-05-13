using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Helpers;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        private class TestApplicationPaths : IApplicationPaths
        {
            public string ProgramDataPath { get; set; } = "ProgramDataPath";
            public string LogDirectoryPath { get; set; } = "LogDirectoryPath";
            public string ConfigurationDirectoryPath { get; set; } = "ConfigurationDirectoryPath";
            public string CachePath { get; set; } = "CachePath";
            public string TempDirectory { get; set; } = "TempDirectory";
            public string WebPath { get; set; } = "WebPath";
            public string ProgramSystemPath { get; set; } = "ProgramSystemPath";
        }

        [Fact]
        public void LogEnvironmentInfo_LogsExpectedInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new TestApplicationPaths();

            var loggedMessages = new List<(string message, object[] args)>();

            loggerMock.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception?, Func<object, Exception?, string>>(
                (level, eventId, state, ex, formatter) =>
                {
                    var formattedMessage = formatter(state, ex);
                    loggedMessages.Add((formattedMessage, new object[] { state }));
                });

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            // We expect at least the call on line 65: logger.LogInformation("Web resources path: {WebPath}", appPaths.WebPath);
            // So check that this message was logged with the correct WebPath value
            Assert.Contains(loggedMessages, m => m.message.Contains("Web resources path:") && m.message.Contains(appPaths.WebPath));

            // Also check some other expected log messages to ensure coverage
            Assert.Contains(loggedMessages, m => m.message.Contains("Environment Variables:"));
            Assert.Contains(loggedMessages, m => m.message.Contains("Arguments:"));
            Assert.Contains(loggedMessages, m => m.message.Contains("Operating system:"));
            Assert.Contains(loggedMessages, m => m.message.Contains("Architecture:"));
            Assert.Contains(loggedMessages, m => m.message.Contains("64-Bit Process:"));
            Assert.Contains(loggedMessages, m => m.message.Contains("User Interactive:"));
            Assert.Contains(loggedMessages, m => m.message.Contains("Processor count:"));
            Assert.Contains(loggedMessages, m => m.message.Contains("Program data path:"));
            Assert.Contains(loggedMessages, m => m.message.Contains("Log directory path:"));
            Assert.Contains(loggedMessages, m => m.message.Contains("Config directory path:"));
            Assert.Contains(loggedMessages, m => m.message.Contains("Cache path:"));
            Assert.Contains(loggedMessages, m => m.message.Contains("Temp directory path:"));
            Assert.Contains(loggedMessages, m => m.message.Contains("Application directory:"));
        }
    }
}
