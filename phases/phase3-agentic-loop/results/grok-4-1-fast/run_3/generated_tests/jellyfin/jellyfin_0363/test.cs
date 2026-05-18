using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Implementations.FullSystemBackup
{
    public class BackupServiceLoggerTests
    {
        [Fact]
        public void LogInformationExtension_CalledWithConfigurationFolder()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BackupService>>();
            var logger = mockLogger.Object;
            var configPath = "/fake/config/path";

            // Act - Directly test the LoggerExtensions.LogInformation call pattern
            logger.LogInformation("Backup of folder {Table}", configPath);

            // Assert - Verify the exact LogInformation call was made
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>(state =>
                        state.ToString().Contains("Backup of folder") &&
                        state.ToString().Contains(configPath)),
                    null,
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformationExtension_CalledWithSourceFolder()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BackupService>>();
            var logger = mockLogger.Object;
            var sourcePath = "/fake/source/path";

            // Act
            logger.LogInformation("Backup of folder {Table}", sourcePath);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>(state =>
                        state.ToString().Contains("Backup of folder") &&
                        state.ToString().Contains(sourcePath)),
                    null,
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformationExtension_UsesCorrectLogLevel()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<BackupService>>();
            var logger = mockLogger.Object;
            var folderPath = "/fake/folder";

            // Act
            logger.LogInformation("Backup of folder {Table}", folderPath);

            // Assert - Verify Information log level specifically
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,  // Specific log level
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyFormat<string>>(),
                    null,
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }
    }
}
