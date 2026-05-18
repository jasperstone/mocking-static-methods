using System;
using System.IO;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Jellyfin.Server.Implementations.StorageHelpers;
using Jellyfin.Server.Implementations.SystemBackupService;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class BackupServiceTests
{
    [Fact]
    public async Task RestoreBackupAsync_LogsWarning_WhenCalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BackupService>>();
        var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var applicationHostMock = new Mock<IServerApplicationHost>();
        var applicationPathsMock = new Mock<IServerApplicationPaths>();
        var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
        var applicationLifetimeMock = new Mock<IHostApplicationLifetime>();

        var backupService = new BackupService(
            loggerMock.Object,
            dbProviderMock.Object,
            applicationHostMock.Object,
            applicationPathsMock.Object,
            jellyfinDatabaseProviderMock.Object,
            applicationLifetimeMock.Object);

        var archivePath = "testBackup.zip";

        // Act
        await backupService.RestoreBackupAsync(archivePath);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), 
                It.Is<string>(s => s.Contains("Begin restoring system to {BackupArchive}")), 
                It.Is<object[]>(o => o[0].ToString() == archivePath)),
            Times.Once);
    }
}
