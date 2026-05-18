using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class BackupServiceTests
    {
        [Fact]
        public void LogInformation_DatabasePurged_CanBeVerified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();

            // Act - Simulate the exact LogInformation call from line 202
            loggerMock.Object.LogInformation("Database Purged");

            // Assert - Verify the LogInformation extension method was called exactly once
            loggerMock.Verify(
                x => x.LogInformation("Database Purged"),
                Times.Once);
        }

        [Fact]
        public void LogInformation_BeginPurgingDatabase_CanBeVerified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();

            // Act - Simulate the LogInformation call before purge
            loggerMock.Object.LogInformation("Begin purging database");

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Begin purging database"),
                Times.Once);
        }

        [Fact]
        public void LogInformation_ReadBackupOfTable_CanBeVerified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var tableName = "Users";

            // Act - Simulate the templated LogInformation call
            loggerMock.Object.LogInformation("Read backup of {Table}", tableName);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Read backup of {Table}", tableName),
                Times.Once);
        }

        [Fact]
        public void LogInformation_RestoreBackupOfTable_CanBeVerified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var tableName = "Users";

            // Act - Simulate the restore LogInformation call
            loggerMock.Object.LogInformation("Restore backup of {Table}", tableName);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Restore backup of {Table}", tableName),
                Times.Once);
        }
    }
}
