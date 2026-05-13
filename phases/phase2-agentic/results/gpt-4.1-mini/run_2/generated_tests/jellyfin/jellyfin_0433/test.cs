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
        public void LogEnvironmentInfo_LogsExpectedInformationIncludingWebPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new TestApplicationPaths();

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            // Verify the specific call on line 65: logger.LogInformation("Web resources path: {WebPath}", appPaths.WebPath);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Web resources path:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Also verify the parameter passed is the WebPath property value
            loggerMock.Verify(
                x => x.LogInformation("Web resources path: {WebPath}", appPaths.WebPath),
                Times.Once);
        }
    }
}
