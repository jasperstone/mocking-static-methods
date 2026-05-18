using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Jellyfin.Server.Helpers;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsWebPathInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logMessages = new List<string>();
            
            mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, It.IsAnyType, Exception?, Func<It.IsAnyType, Exception?, string>>((level, eventId, state, ex, formatter) =>
                {
                    var message = formatter(state, ex);
                    logMessages.Add(message);
                });

            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(x => x.WebPath).Returns("/path/to/web");
            // Setup other required properties to prevent exceptions
            mockAppPaths.SetupGet(x => x.ProgramDataPath).Returns("/programdata");
            mockAppPaths.SetupGet(x => x.LogDirectoryPath).Returns("/logs");
            mockAppPaths.SetupGet(x => x.ConfigurationDirectoryPath).Returns("/config");
            mockAppPaths.SetupGet(x => x.CachePath).Returns("/cache");
            mockAppPaths.SetupGet(x => x.TempDirectory).Returns("/temp");
            mockAppPaths.SetupGet(x => x.ProgramSystemPath).Returns("/system");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert
            var webLogMessage = Assert.Single(logMessages, msg => msg.Contains("Web resources path: {WebPath}") || msg.Contains("/path/to/web"));
            Assert.Contains("Web resources path", webLogMessage);
            Assert.Contains("/path/to/web", webLogMessage);
        }
    }
}
