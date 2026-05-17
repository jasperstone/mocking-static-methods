using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using Jellyfin.Server.Helpers;
using System.Runtime.InteropServices;

namespace Jellyfin.Server.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsEnvironmentVariables()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths("data", "log", "config", "cache", "web");
            var envVars = new Dictionary<object, object> { { "key", "value" } };

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Environment Variables: {EnvVars}", envVars), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsArguments()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths("data", "log", "config", "cache", "web");
            var args = new[] { "arg1", "arg2" };

            // Act
            Environment.SetEnvironmentVariable("JELLYFIN_ARGS", string.Join(" ", args));
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Arguments: {Args}", args), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsOperatingSystem()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths("data", "log", "config", "cache", "web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Operating system: {OS}", RuntimeInformation.OSDescription), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsArchitecture()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths("data", "log", "config", "cache", "web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Architecture: {Architecture}", RuntimeInformation.OSArchitecture), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_Logs64BitProcess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths("data", "log", "config", "cache", "web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation("64-Bit Process: {Is64Bit}", Environment.Is64BitProcess), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsUserInteractive()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths("data", "log", "config", "cache", "web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation("User Interactive: {IsUserInteractive}", Environment.UserInteractive), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsProcessorCount()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths("data", "log", "config", "cache", "web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Processor count: {ProcessorCount}", Environment.ProcessorCount), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsProgramDataPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths("data", "log", "config", "cache", "web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Program data path: {ProgramDataPath}", appPaths.ProgramDataPath), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsLogDirectoryPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths("data", "log", "config", "cache", "web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Log directory path: {LogDirectoryPath}", appPaths.LogDirectoryPath), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsConfigurationDirectoryPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths("data", "log", "config", "cache", "web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Config directory path: {ConfigurationDirectoryPath}", appPaths.ConfigurationDirectoryPath), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsCachePath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths("data", "log", "config", "cache", "web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Cache path: {CachePath}", appPaths.CachePath), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsTempDirectory()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths("data", "log", "config", "cache", "web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Temp directory path: {TempDirPath}", appPaths.TempDirectory), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsWebResourcesPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths("data", "log", "config", "cache", "web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Web resources path: {WebPath}", appPaths.WebPath), Times.Once);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsApplicationDirectory()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths("data", "log", "config", "cache", "web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Application directory: {ApplicationPath}", appPaths.ProgramSystemPath), Times.Once);
        }
    }
}
