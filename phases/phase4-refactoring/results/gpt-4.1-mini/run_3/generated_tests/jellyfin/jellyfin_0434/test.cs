using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Helpers;

namespace Jellyfin.Server.Tests.Helpers
{
    public class LoggerExtensionsTests
    {
        private class FakeAppPaths : IApplicationPaths
        {
            public string ProgramDataPath => "ProgramDataPath";
            public string LogDirectoryPath => "LogDirectoryPath";
            public string ConfigurationDirectoryPath => "ConfigurationDirectoryPath";
            public string CachePath => "CachePath";
            public string TempDirectory => "TempDirectory";
            public string WebPath => "WebPath";
            public string ProgramSystemPath => "ProgramSystemPath";
        }

        [Fact]
        public void LogEnvironmentInfo_LogsExpectedInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new FakeAppPaths();

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Application directory:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.LogInformation("Environment Variables: {EnvVars}", It.IsAny<IDictionary<object, object>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.LogInformation("Arguments: {Args}", It.IsAny<IEnumerable<string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.LogInformation("Operating system: {OS}", It.IsAny<string>()),
                Times.Once);

            loggerMock.Verify(
                x => x.LogInformation("Architecture: {Architecture}", It.IsAny<object>()),
                Times.Once);
        }
    }
}
