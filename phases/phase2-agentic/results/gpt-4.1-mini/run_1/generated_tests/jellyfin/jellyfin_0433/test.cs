using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Helpers.Tests
{
    public interface IApplicationPaths
    {
        string ProgramDataPath { get; }
        string LogDirectoryPath { get; }
        string ConfigurationDirectoryPath { get; }
        string CachePath { get; }
        string TempDirectory { get; }
        string WebPath { get; }
        string ProgramSystemPath { get; }
    }

    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsExpectedInformationIncludingWebPath()
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

            var loggedMessages = new List<(string message, object[] args)>();

            loggerMock.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Callback<LogLevel, EventId, object, Exception, Delegate>((level, eventId, state, ex, formatter) =>
            {
                var formattedMessage = formatter.DynamicInvoke(state, ex) as string;
                loggedMessages.Add((formattedMessage, new object[] { state }));
            });

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPathsMock.Object);

            // Assert
            // Check that the log containing the WebPath is present
            Assert.Contains(loggedMessages, log =>
                log.message.Contains("Web resources path:") &&
                log.message.Contains("WebPathValue"));

            // Check that the log containing the ApplicationPath is present
            Assert.Contains(loggedMessages, log =>
                log.message.Contains("Application directory:") &&
                log.message.Contains("ProgramSystemPathValue"));

            // Check that the logger was called at least once with LogInformation
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
