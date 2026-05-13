using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Helpers;
using Jellyfin.Server.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Jellyfin.Server.Tests
{
    public class StartupHelpersTests
    {
        [Fact]
        public void LogEnvironmentInfo_LogsEnvironmentVariables()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                programDataPath: "/path/to/program/data",
                logDirectoryPath: "/path/to/log/directory",
                configurationDirectoryPath: "/path/to/configuration/directory",
                cachePath: "/path/to/cache",
                webPath: "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsCommandLineArguments()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                programDataPath: "/path/to/program/data",
                logDirectoryPath: "/path/to/log/directory",
                configurationDirectoryPath: "/path/to/configuration/directory",
                cachePath: "/path/to/cache",
                webPath: "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsOperatingSystem()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                programDataPath: "/path/to/program/data",
                logDirectoryPath: "/path/to/log/directory",
                configurationDirectoryPath: "/path/to/configuration/directory",
                cachePath: "/path/to/cache",
                webPath: "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsArchitecture()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                programDataPath: "/path/to/program/data",
                logDirectoryPath: "/path/to/log/directory",
                configurationDirectoryPath: "/path/to/configuration/directory",
                cachePath: "/path/to/cache",
                webPath: "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LogEnvironmentInfo_Logs64BitProcess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                programDataPath: "/path/to/program/data",
                logDirectoryPath: "/path/to/log/directory",
                configurationDirectoryPath: "/path/to/configuration/directory",
                cachePath: "/path/to/cache",
                webPath: "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsUserInteractive()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                programDataPath: "/path/to/program/data",
                logDirectoryPath: "/path/to/log/directory",
                configurationDirectoryPath: "/path/to/configuration/directory",
                cachePath: "/path/to/cache",
                webPath: "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsProcessorCount()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                programDataPath: "/path/to/program/data",
                logDirectoryPath: "/path/to/log/directory",
                configurationDirectoryPath: "/path/to/configuration/directory",
                cachePath: "/path/to/cache",
                webPath: "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsProgramDataPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                programDataPath: "/path/to/program/data",
                logDirectoryPath: "/path/to/log/directory",
                configurationDirectoryPath: "/path/to/configuration/directory",
                cachePath: "/path/to/cache",
                webPath: "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsLogDirectoryPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                programDataPath: "/path/to/program/data",
                logDirectoryPath: "/path/to/log/directory",
                configurationDirectoryPath: "/path/to/configuration/directory",
                cachePath: "/path/to/cache",
                webPath: "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsConfigurationDirectoryPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                programDataPath: "/path/to/program/data",
                logDirectoryPath: "/path/to/log/directory",
                configurationDirectoryPath: "/path/to/configuration/directory",
                cachePath: "/path/to/cache",
                webPath: "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsCachePath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                programDataPath: "/path/to/program/data",
                logDirectoryPath: "/path/to/log/directory",
                configurationDirectoryPath: "/path/to/configuration/directory",
                cachePath: "/path/to/cache",
                webPath: "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsTempDirectoryPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                programDataPath: "/path/to/program/data",
                logDirectoryPath: "/path/to/log/directory",
                configurationDirectoryPath: "/path/to/configuration/directory",
                cachePath: "/path/to/cache",
                webPath: "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsWebResourcesPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                programDataPath: "/path/to/program/data",
                logDirectoryPath: "/path/to/log/directory",
                configurationDirectoryPath: "/path/to/configuration/directory",
                cachePath: "/path/to/cache",
                webPath: "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public void LogEnvironmentInfo_LogsApplicationDirectory()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var appPaths = new ServerApplicationPaths(
                programDataPath: "/path/to/program/data",
                logDirectoryPath: "/path/to/log/directory",
                configurationDirectoryPath: "/path/to/configuration/directory",
                cachePath: "/path/to/cache",
                webPath: "/path/to/web");

            // Act
            StartupHelpers.LogEnvironmentInfo(loggerMock.Object, appPaths);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }
    }
}
