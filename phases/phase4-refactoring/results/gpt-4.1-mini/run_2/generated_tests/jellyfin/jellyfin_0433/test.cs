using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Helpers;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        private class TestAppPaths : IApplicationPaths
        {
            public string ProgramDataPath => "/program/data";
            public string LogDirectoryPath => "/log/dir";
            public string ConfigurationDirectoryPath => "/config/dir";
            public string CachePath => "/cache/path";
            public string TempDirectory => "/temp/dir";
            public string WebPath => "/web/path";
            public string ProgramSystemPath => "/program/system";
        }

        [Fact]
        public void LogEnvironmentInfo_LogsWebPathAndOtherInfo()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new TestAppPaths();

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Web resources path: {WebPath}", appPaths.WebPath),
                Times.Once);

            loggerMock.Verify(
                x => x.LogInformation("Program data path: {ProgramDataPath}", appPaths.ProgramDataPath),
                Times.Once);

            loggerMock.Verify(
                x => x.LogInformation("Arguments: {Args}", It.IsAny<IEnumerable<string>>()),
                Times.Once);
        }
    }
}
