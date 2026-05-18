using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class BackupServiceTests
{
    [Fact]
    public async Task LogInformation_ShouldBeCalled_WithCorrectParameters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BackupService>>();
        var mockDbProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var mockApplicationHost = new Mock<IServerApplicationHost>();
        var mockApplicationPaths = new Mock<IServerApplicationPaths>();
        var mockJellyfinDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
        var mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();

        var backupService = new BackupService(
            mockLogger.Object,
            mockDbProvider.Object,
            mockApplicationHost.Object,
            mockApplicationPaths.Object,
            mockJellyfinDatabaseProvider.Object,
            mockHostApplicationLifetime.Object);

        // Simulate the application paths
        mockApplicationPaths.Setup(p => p.ConfigurationDirectoryPath).Returns("TestConfigPath");

        // Act
        await backupService.CreateBackupAsync(new BackupOptions { Subtitles = false });

        // Assert
        mockLogger.Verify(
            logger => logger.LogInformation(
                It.Is<string>(s => s.Contains("Backup of folder {Table}")),
                It.Is<object[]>(o => o[0].ToString() == "TestConfigPath")),
            Times.Once);
    }
}
