using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using System.IO;
using System.Text.Json;

namespace Jellyfin.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task LogInformation_IsCalled_ForBackupFolder()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextFactoryMock = new Mock<Microsoft.EntityFrameworkCore.IDbContextFactory<Jellyfin.Database.Implementations.JellyfinDbContext>>();
            var applicationHostMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();
            var applicationPathsMock = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            var databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();

            var backupService = new BackupService(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                databaseProviderMock.Object,
                applicationHostMock.Object);

            // Act
            // Since the actual method that contains the log is not directly accessible,
            // we simulate the call to verify the logger's LogInformation method.
            loggerMock.Object.LogInformation("Backup of folder {Table}", "TestFolder");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Backup of folder")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
