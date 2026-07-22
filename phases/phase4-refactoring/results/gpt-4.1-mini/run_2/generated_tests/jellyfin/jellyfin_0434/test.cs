using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common.Configuration;
using Jellyfin.Server.Helpers;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        private class TestAppPaths : IApplicationPaths
        {
            public string ProgramDataPath => "ProgramDataPath";
            public string LogDirectoryPath => "LogDirectoryPath";
            public string ConfigurationDirectoryPath => "ConfigurationDirectoryPath";
            public string CachePath => "CachePath";
            public string TempDirectory => "TempDirectory";
            public string WebPath => "WebPath";
            public string ProgramSystemPath => "ProgramSystemPath";

            // Implementing minimal required members to satisfy interface
            public void MakeSanityCheckOrThrow() { }
            public bool CreateAndCheckMarker(string markerName, string markerValue, bool throwOnError) => true;
            public string DataPath => "DataPath";
            public string ImageCachePath => "ImageCachePath";
            public string PluginsPath => "PluginsPath";
            public string PluginConfigurationsPath => "PluginConfigurationsPath";
        }

        [Fact]
        public void LogEnvironmentInfo_LogsExpectedInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new TestAppPaths();

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            // Verify that LogInformation was called with the expected message containing "Application directory"
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Application directory")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
