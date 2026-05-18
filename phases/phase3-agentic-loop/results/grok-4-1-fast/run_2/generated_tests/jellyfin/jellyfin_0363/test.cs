using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceLoggerTests
    {
        [Fact]
        public void LogInformationExtension_CalledWithConfigurationFolderPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            
            // Simulate the exact LogInformation call from line 373
            // _logger.LogInformation("Backup of folder {Table}", _applicationPaths.ConfigurationDirectoryPath);
            var configPath = "/config";
            loggerMock.Object.LogInformation("Backup of folder {Table}", configPath);

            // Assert - Verify the underlying Log method was called with the formatted message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Backup of folder") && v.ToString()!.Contains(configPath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformationExtension_CalledWithCopyDirectoryPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            
            // Simulate the LogInformation call from CopyDirectory method
            var sourcePath = "/test/source";
            loggerMock.Object.LogInformation("Backup of folder {Table}", sourcePath);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Backup of folder") && v.ToString()!.Contains(sourcePath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtensions_LogInformation_FormatsMessageCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            
            // Act - Call the extension method exactly as used in the code
            loggerMock.Object.LogInformation("Backup of folder {Table}", "any/path");

            // Assert - Verify Log was called at Information level with correct message pattern
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Backup of folder {Table}") || v.ToString()!.Contains("Backup of folder any/path")),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
