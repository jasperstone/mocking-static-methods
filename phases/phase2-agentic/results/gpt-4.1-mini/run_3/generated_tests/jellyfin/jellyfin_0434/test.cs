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
        public void LogEnvironmentInfo_LogsExpectedInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new TestApplicationPaths();

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            // We expect LogInformation to be called with the specific messages and parameters.
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
