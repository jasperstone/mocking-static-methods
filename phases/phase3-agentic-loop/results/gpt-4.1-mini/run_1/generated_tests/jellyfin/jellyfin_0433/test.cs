using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Helpers;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        private class DummyApplicationPaths : IApplicationPaths
        {
            public string ProgramDataPath => "ProgramDataPath";
            public string LogDirectoryPath => "LogDirectoryPath";
            public string ConfigurationDirectoryPath => "ConfigurationDirectoryPath";
            public string CachePath => "CachePath";
            public string TempDirectory => "TempDirectory";
            public string WebPath => "WebPath";
            public string ProgramSystemPath => "ProgramSystemPath";
        }

        [Fact]
        public void LogEnvironmentInfo_LogsExpectedInformationIncludingWebPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new DummyApplicationPaths();

            var loggedMessages = new List<string>();

            loggerMock.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()))
            .Callback((LogLevel level, EventId eventId, object state, System.Exception? ex, Func<object, System.Exception?, string> formatter) =>
            {
                var message = formatter(state, ex);
                loggedMessages.Add(message);
            });

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            Assert.Contains(loggedMessages, m => m.Contains("Web resources path: WebPath"));
            Assert.Contains(loggedMessages, m => m.Contains("Environment Variables:"));
            Assert.Contains(loggedMessages, m => m.Contains("Arguments:"));
            Assert.Contains(loggedMessages, m => m.Contains("Operating system:"));
            Assert.Contains(loggedMessages, m => m.Contains("Architecture:"));
            Assert.Contains(loggedMessages, m => m.Contains("64-Bit Process:"));
            Assert.Contains(loggedMessages, m => m.Contains("User Interactive:"));
            Assert.Contains(loggedMessages, m => m.Contains("Processor count:"));
            Assert.Contains(loggedMessages, m => m.Contains("Program data path: ProgramDataPath"));
            Assert.Contains(loggedMessages, m => m.Contains("Log directory path: LogDirectoryPath"));
            Assert.Contains(loggedMessages, m => m.Contains("Config directory path: ConfigurationDirectoryPath"));
            Assert.Contains(loggedMessages, m => m.Contains("Cache path: CachePath"));
            Assert.Contains(loggedMessages, m => m.Contains("Temp directory path: TempDirectory"));
            Assert.Contains(loggedMessages, m => m.Contains("Application directory: ProgramSystemPath"));
        }
    }
}
