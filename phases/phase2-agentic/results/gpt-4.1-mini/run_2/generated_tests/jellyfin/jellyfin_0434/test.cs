using System;
using System.Collections.Generic;
using System.Linq;
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
            public string ProgramDataPath { get; set; } = "/data";
            public string LogDirectoryPath { get; set; } = "/log";
            public string ConfigurationDirectoryPath { get; set; } = "/config";
            public string CachePath { get; set; } = "/cache";
            public string TempDirectory { get; set; } = "/temp";
            public string WebPath { get; set; } = "/web";
            public string ProgramSystemPath { get; set; } = "/app";
        }

        [Fact]
        public void LogEnvironmentInfo_LogsExpectedInformationIncludingApplicationPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new TestApplicationPaths();

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            // Verify that LogInformation was called with the ApplicationPath message and the correct path
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Application directory:") && v.ToString().Contains(appPaths.ProgramSystemPath)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Also verify some other key log calls to ensure coverage
            loggerMock.Verify(
                x => x.LogInformation("Environment Variables: {EnvVars}", It.IsAny<IDictionary<object, object>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.LogInformation("Arguments: {Args}", It.IsAny<IEnumerable<string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.LogInformation("Operating system: {OS}", It.IsAny<string>()),
                Times.Once);
        }
    }
}
