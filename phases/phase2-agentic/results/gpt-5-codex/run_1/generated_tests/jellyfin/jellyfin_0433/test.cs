using System;
using System.Collections;
using System.Collections.Generic;
using Jellyfin.Server.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Helpers
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsExpectedEnvironmentDetails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPathsMock = new Mock<IApplicationPaths>();

            appPathsMock.SetupGet(p => p.ProgramDataPath).Returns("/data");
            appPathsMock.SetupGet(p => p.LogDirectoryPath).Returns("/logs");
            appPathsMock.SetupGet(p => p.ConfigurationDirectoryPath).Returns("/config");
            appPathsMock.SetupGet(p => p.CachePath).Returns("/cache");
            appPathsMock.SetupGet(p => p.TempDirectory).Returns("/temp");
            appPathsMock.SetupGet(p => p.WebPath).Returns("/web");
            appPathsMock.SetupGet(p => p.ProgramSystemPath).Returns("/app");

            var loggedMessages = new List<string>();
            loggerMock
                .Setup(l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                    {
                        loggedMessages.Add(state.ToString()!);
                        return true;
                    }),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Act
            LoggerExtensions.LogEnvironmentInfo(loggerMock.Object, appPathsMock.Object);

            // Assert
            Assert.Contains("Web resources path: /web", loggedMessages);
        }
    }
}
