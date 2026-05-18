using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Jellyfin.Server.Implementations.SystemBackupService;
using MediaBrowser.Controller;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

// Define a public version of BackupOptions for testing purposes
public class TestBackupOptions
{
    public bool Metadata { get; set; }
    public bool Trickplay { get; set; }
    public bool Subtitles { get; set; }
    public bool Database { get; set; }
}

public class BackupServiceTests
{
    private const string LogMessageTemplate = "Backup of folder {Table}";

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

        // Setup mock paths
        mockApplicationPaths.Setup(p => p.ConfigurationDirectoryPath).Returns("mockConfigPath");

        // Act
        await backupService.CreateBackupAsync(new TestBackupOptions { Subtitles = false });

        // Assert
        mockLogger.Verify(
            logger => logger.LogInformation(
                LogMessageTemplate,
                It.Is<object[]>(o => o[0].ToString() == "mockConfigPath")),
            Times.Once);
    }
}
