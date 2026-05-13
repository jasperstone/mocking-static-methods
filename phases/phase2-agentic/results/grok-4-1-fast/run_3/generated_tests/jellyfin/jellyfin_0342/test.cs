using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup;

public class BackupServiceTests
{
    private const string TestArchivePath = "test-backup.zip";
    private const string ManifestEntryName = "manifest.json";

    [Fact]
    public async Task RestoreBackupAsync_LogsWarningAtStart()
    {
        // Arrange
        var logger = new Mock<ILogger<BackupService>>();
        var dbProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var applicationHost = new Mock<IServerApplicationHost>();
        var applicationPaths = new Mock<IServerApplicationPaths>();
        var jellyfinDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
        var applicationLifetime = new Mock<IHostApplicationLifetime>();

        applicationPaths.Setup(x => x.ConfigurationDirectoryPath).Returns("/config");
        applicationPaths.Setup(x => x.DataPath).Returns("/data");
        applicationPaths.Setup(x => x.RootFolderPath).Returns("/root");
        applicationPaths.Setup(x => x.InternalMetadataPath).Returns("/data/metadata");
        applicationPaths.Setup(x => x.DefaultInternalMetadataPath).Returns("/data/metadata-default");

        applicationHost.Setup(x => x.ApplicationVersion).Returns(new Version(10, 8, 0));

        // Create a minimal valid zip file
        File.WriteAllBytes(TestArchivePath, new byte[0]);
        using var zip = ZipFile.Open(TestArchivePath, ZipArchiveMode.Create);
        var manifestEntry = zip.CreateEntry(ManifestEntryName);
        using var manifestStream = manifestEntry.Open();
        var manifestJson = JsonSerializer.Serialize(new BackupManifest
        {
            ServerVersion = new Version(10, 7, 0),
            BackupEngineVersion = new Version(0, 2, 0),
            Options = new BackupOptions { Database = false }
        });
        await using var writer = new StreamWriter(manifestStream);
        await writer.WriteAsync(manifestJson);

        var service = new BackupService(logger.Object, dbProvider.Object, applicationHost.Object,
            applicationPaths.Object, jellyfinDatabaseProvider.Object, applicationLifetime.Object);

        // Act
        await service.RestoreBackupAsync(TestArchivePath);

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Begin restoring system to") && v.ToString().Contains(TestArchivePath)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RestoreBackupAsync_FileNotFound_ThrowsException()
    {
        // Arrange
        var logger = new Mock<ILogger<BackupService>>();
        var dbProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var applicationHost = new Mock<IServerApplicationHost>();
        var applicationPaths = new Mock<IServerApplicationPaths>();
        var jellyfinDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
        var applicationLifetime = new Mock<IHostApplicationLifetime>();

        var service = new BackupService(logger.Object, dbProvider.Object, applicationHost.Object,
            applicationPaths.Object, jellyfinDatabaseProvider.Object, applicationLifetime.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => service.RestoreBackupAsync("nonexistent.zip"));
        Assert.Contains("does not exist", exception.Message);
    }

    public class BackupManifest
    {
        public Version ServerVersion { get; set; } = null!;
        public Version BackupEngineVersion { get; set; } = null!;
        public BackupOptions Options { get; set; } = null!;
    }

    public class BackupOptions
    {
        public bool Database { get; set; }
    }
}
