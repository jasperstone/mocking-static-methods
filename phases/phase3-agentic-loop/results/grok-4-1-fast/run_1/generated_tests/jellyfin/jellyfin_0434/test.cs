using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Jellyfin.Server.Helpers;
using MediaBrowser.Controller;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_CallsApplicationDirectoryLogInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IServerApplicationPaths>();
            mockAppPaths.SetupGet(x => x.ProgramSystemPath).Returns("/test/application/path");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert - verify the specific LogInformation call on line 66
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Application directory: {ApplicationPath}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_ExecutesAllExpectedLogStatements()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockAppPaths = new Mock<IServerApplicationPaths>();
            mockAppPaths.SetupGet(x => x.ProgramDataPath).Returns("/data");
            mockAppPaths.SetupGet(x => x.LogDirectoryPath).Returns("/log");
            mockAppPaths.SetupGet(x => x.ConfigurationDirectoryPath).Returns("/config");
            mockAppPaths.SetupGet(x => x.CachePath).Returns("/cache");
            mockAppPaths.SetupGet(x => x.TempDirectory).Returns("/temp");
            mockAppPaths.SetupGet(x => x.WebPath).Returns("/web");
            mockAppPaths.SetupGet(x => x.ProgramSystemPath).Returns("/app");

            // Act
            StartupHelpers.LogEnvironmentInfo(mockLogger.Object, mockAppPaths.Object);

            // Assert - verify total number of Information log calls (14 expected)
            mockLogger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Exactly(14));
        }
    }
}
