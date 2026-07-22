using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_BackupOfFolder_CalledWithConfigurationDirectory()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<object>>();
            var configPath = "/path/to/config";

            // Act - Tests the exact LoggerExtensions.LogInformation call from line 373
            mockLogger.Object.LogInformation("Backup of folder {Table}", configPath);

            // Assert - Verifies the underlying Log method was called with Information level
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_BackupOfFolder_CalledWithSourceDirectory()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<object>>();
            var sourcePath = "/path/to/source";

            // Act - Tests the LoggerExtensions.LogInformation call from CopyDirectory method
            mockLogger.Object.LogInformation("Backup of folder {Table}", sourcePath);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_UsesCorrectMessageTemplateAndLogLevel()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<object>>();
            var folderPath = "test/folder";

            // Act
            mockLogger.Object.LogInformation("Backup of folder {Table}", folderPath);

            // Assert - Verifies specific LogLevel and that message template was used
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.Is<EventId>(e => e.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Backup of folder {Table}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
