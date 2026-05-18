using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Helpers;

namespace Jellyfin.Server.Tests.Helpers
{
    public class StartupHelpersTests
    {
        private class TestApplicationPaths
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
            var appPaths = new TestApplicationPaths();

            // Act
            // Use reflection to create a dynamic proxy for the appPaths to bypass interface requirements
            var proxy = new
            {
                ProgramDataPath = appPaths.ProgramDataPath,
                LogDirectoryPath = appPaths.LogDirectoryPath,
                ConfigurationDirectoryPath = appPaths.ConfigurationDirectoryPath,
                CachePath = appPaths.CachePath,
                TempDirectory = appPaths.TempDirectory,
                WebPath = appPaths.WebPath,
                ProgramSystemPath = appPaths.ProgramSystemPath
            };

            // Use dynamic to call the method
            dynamic dynamicAppPaths = proxy;
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, dynamicAppPaths);

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
