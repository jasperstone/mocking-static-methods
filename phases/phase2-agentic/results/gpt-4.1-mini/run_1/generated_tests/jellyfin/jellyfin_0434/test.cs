using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Helpers;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsExpectedInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPathsMock = new Mock<IApplicationPaths>();

            appPathsMock.SetupGet(a => a.ProgramDataPath).Returns("ProgramDataPathValue");
            appPathsMock.SetupGet(a => a.LogDirectoryPath).Returns("LogDirectoryPathValue");
            appPathsMock.SetupGet(a => a.ConfigurationDirectoryPath).Returns("ConfigurationDirectoryPathValue");
            appPathsMock.SetupGet(a => a.CachePath).Returns("CachePathValue");
            appPathsMock.SetupGet(a => a.TempDirectory).Returns("TempDirectoryValue");
            appPathsMock.SetupGet(a => a.WebPath).Returns("WebPathValue");
            appPathsMock.SetupGet(a => a.ProgramSystemPath).Returns("ProgramSystemPathValue");

            var loggedMessages = new List<string>();
            loggerMock.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<System.Exception>(),
                It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()))
                .Callback<LogLevel, EventId, object, System.Exception, Func<object, System.Exception?, string>>(
                    (level, eventId, state, exception, formatter) =>
                    {
                        var message = formatter(state, exception);
                        loggedMessages.Add(message);
                    });

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPathsMock.Object);

            // Assert
            // We expect at least the last log call with "Application directory: {ApplicationPath}" to be logged
            Assert.Contains(loggedMessages, m => m.Contains("Application directory: ProgramSystemPathValue"));
            // Also check some other expected log messages
            Assert.Contains(loggedMessages, m => m.Contains("Environment Variables:"));
            Assert.Contains(loggedMessages, m => m.Contains("Arguments:"));
            Assert.Contains(loggedMessages, m => m.Contains("Operating system:"));
            Assert.Contains(loggedMessages, m => m.Contains("Architecture:"));
            Assert.Contains(loggedMessages, m => m.Contains("64-Bit Process:"));
            Assert.Contains(loggedMessages, m => m.Contains("User Interactive:"));
            Assert.Contains(loggedMessages, m => m.Contains("Processor count:"));
            Assert.Contains(loggedMessages, m => m.Contains("Program data path: ProgramDataPathValue"));
            Assert.Contains(loggedMessages, m => m.Contains("Log directory path: LogDirectoryPathValue"));
            Assert.Contains(loggedMessages, m => m.Contains("Config directory path: ConfigurationDirectoryPathValue"));
            Assert.Contains(loggedMessages, m => m.Contains("Cache path: CachePathValue"));
            Assert.Contains(loggedMessages, m => m.Contains("Temp directory path: TempDirectoryValue"));
            Assert.Contains(loggedMessages, m => m.Contains("Web resources path: WebPathValue"));
        }
    }
}
