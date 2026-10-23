using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Jellyfin.Server.Helpers;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsWebResourcesPath()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception?, string>>((level, eventId, state, ex, formatter) =>
                {
                    var message = formatter(state, ex);
                    Assert.Contains("Web resources path: {WebPath}", message);
                });

            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(x => x.WebPath).Returns("/path/to/web/resources");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsAllExpectedEnvironmentInformation()
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
                .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception?, string>>((level, eventId, state, ex, formatter) =>
                {
                    var message = formatter(state, ex);
                    if (level == LogLevel.Information)
                    {
                        logMessages.Add(message);
                    }
                });

            var mockAppPaths = new Mock<IApplicationPaths>();

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert
            var expectedMessages = new[]
            {
                "Environment Variables: {EnvVars}",
                "Arguments: {Args}",
                "Operating system: {OS}",
                "Architecture: {Architecture}",
                "64-Bit Process: {Is64Bit}",
                "User Interactive: {IsUserInteractive}",
                "Processor count: {ProcessorCount}",
                "Program data path: {ProgramDataPath}",
                "Log directory path: {LogDirectoryPath}",
                "Config directory path: {ConfigurationDirectoryPath}",
                "Cache path: {CachePath}",
                "Temp directory path: {TempDirPath}",
                "Web resources path: {WebPath}",
                "Application directory: {ApplicationPath}"
            };

            foreach (var expected in expectedMessages)
            {
                Assert.Contains(logMessages, msg => msg.Contains(expected));
            }
        }
    }
}
