using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Helpers.Tests
{
    // Minimal interface definition to allow compilation of the test
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

    public class LoggerExtensionsTests
    {
        private class FakeApplicationPaths : IApplicationPaths
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
        public void LogEnvironmentInfo_LogsExpectedInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new FakeApplicationPaths();

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Environment Variables: {EnvVars}", It.IsAny<IDictionary<object, object>>()), Times.Once);
            loggerMock.Verify(l => l.LogInformation("Arguments: {Args}", It.IsAny<IEnumerable<string>>()), Times.Once);
            loggerMock.Verify(l => l.LogInformation("Operating system: {OS}", It.IsAny<string>()), Times.Once);
            loggerMock.Verify(l => l.LogInformation("Architecture: {Architecture}", It.IsAny<object>()), Times.Once);
            loggerMock.Verify(l => l.LogInformation("64-Bit Process: {Is64Bit}", It.IsAny<bool>()), Times.Once);
            loggerMock.Verify(l => l.LogInformation("User Interactive: {IsUserInteractive}", It.IsAny<bool>()), Times.Once);
            loggerMock.Verify(l => l.LogInformation("Processor count: {ProcessorCount}", It.IsAny<int>()), Times.Once);
            loggerMock.Verify(l => l.LogInformation("Program data path: {ProgramDataPath}", appPaths.ProgramDataPath), Times.Once);
            loggerMock.Verify(l => l.LogInformation("Log directory path: {LogDirectoryPath}", appPaths.LogDirectoryPath), Times.Once);
            loggerMock.Verify(l => l.LogInformation("Config directory path: {ConfigurationDirectoryPath}", appPaths.ConfigurationDirectoryPath), Times.Once);
            loggerMock.Verify(l => l.LogInformation("Cache path: {CachePath}", appPaths.CachePath), Times.Once);
            loggerMock.Verify(l => l.LogInformation("Temp directory path: {TempDirPath}", appPaths.TempDirectory), Times.Once);
            loggerMock.Verify(l => l.LogInformation("Web resources path: {WebPath}", appPaths.WebPath), Times.Once);
            loggerMock.Verify(l => l.LogInformation("Application directory: {ApplicationPath}", appPaths.ProgramSystemPath), Times.Once);
        }
    }
}
