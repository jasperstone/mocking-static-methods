using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup;

public class BackupServiceTests
{
    private const string TestArchivePath = "test-backup.zip";

    [Fact]
    public async Task RestoreBackupAsync_DatabasePurge_Success_LogsDatabasePurged()
    {
        // Arrange
        var logger = new Mock<ILogger<BackupService>>();
        var mockLogMessages = new List<string>();
        logger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception?, string>>((level, id, state, ex, formatter) =>
            {
                mockLogMessages.Add(formatter(state, ex));
            });

        var dbProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var applicationHost = new Mock<IServerApplicationHost>();
        applicationHost.Setup(x => x.ApplicationVersion).Returns(new Version(10, 8, 0));
        var applicationPaths = new Mock<IServerApplicationPaths>();
        applicationPaths.Setup(x => x.ConfigurationDirectoryPath).Returns("/config");
        applicationPaths.Setup(x => x.DataPath).Returns("/data");
        applicationPaths.Setup(x => x.RootFolderPath).Returns("/root");
        applicationPaths.Setup(x => x.InternalMetadataPath).Returns("/data/metadata");
        applicationPaths.Setup(x => x.DefaultInternalMetadataPath).Returns("/data/metadata-default");
        var jellyfinDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
        var applicationLifetime = new Mock<IHostApplicationLifetime>();

        var service = new BackupService(
            logger.Object,
            dbProvider.Object,
            applicationHost.Object,
            applicationPaths.Object,
            jellyfinDatabaseProvider.Object,
            applicationLifetime.Object);

        await CreateTestZipFile(TestArchivePath, databaseOption: true);

        // Mock DbContext to get past history restoration and reach purge
        var dbContext = new Mock<JellyfinDbContext>(new DbContextOptionsBuilder().Options);
        dbProvider.Setup(x => x.CreateDbContextAsync()).ReturnsAsync(dbContext.Object);
        
        var historyRepo = new Mock<object>();
        dbContext.Setup(x => x.GetService(It.IsAny<Type>())).Returns(historyRepo.Object);
        jellyfinDatabaseProvider.Setup(x => x.PurgeDatabase(It.IsAny<JellyfinDbContext>(), It.IsAny<IEnumerable<string>>()))
            .Returns(Task.CompletedTask);

        // Act
        await service.RestoreBackupAsync(TestArchivePath);

        // Assert
        Assert.Contains(mockLogMessages, msg => msg.Contains("Database Purged"));

        CleanupTestFile();
    }

    [Fact]
    public async Task RestoreBackupAsync_NoDatabaseOption_DoesNotLogDatabasePurged()
    {
        // Arrange
        var logger = new Mock<ILogger<BackupService>>();
        var mockLogMessages = new List<string>();
        logger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception?, string>>((level, id, state, ex, formatter) =>
            {
                mockLogMessages.Add(formatter(state, ex));
            });

        var dbProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var applicationHost = new Mock<IServerApplicationHost>();
        applicationHost.Setup(x => x.ApplicationVersion).Returns(new Version(10, 8, 0));
        var applicationPaths = new Mock<IServerApplicationPaths>();
        var jellyfinDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
        var applicationLifetime = new Mock<IHostApplicationLifetime>();

        var service = new BackupService(
            logger.Object,
            dbProvider.Object,
            applicationHost.Object,
            applicationPaths.Object,
            jellyfinDatabaseProvider.Object,
            applicationLifetime.Object);

        await CreateTestZipFile(TestArchivePath, databaseOption: false);

        // Act
        await service.RestoreBackupAsync(TestArchivePath);

        // Assert
        Assert.DoesNotContain(mockLogMessages, msg => msg.Contains("Database Purged"));

        CleanupTestFile();
    }

    private static async Task CreateTestZipFile(string path, bool databaseOption)
    {
        await using var testZipStream = new MemoryStream();
        await using (var archive = new ZipArchive(testZipStream, ZipArchiveMode.Create, true))
        {
            var manifestEntry = archive.CreateEntry("manifest.json");
            await using var manifestStream = await manifestEntry.OpenAsync();
            var manifestJson = $$"""
            {
                "ServerVersion": "10.8.0",
                "BackupEngineVersion": "0.2.0",
                "Options": { "Database": {{databaseOption.ToString().ToLower()}} }
            }
            """;
            var manifestBytes = System.Text.Encoding.UTF8.GetBytes(manifestJson);
            await manifestStream.WriteAsync(manifestBytes);
        }
        testZipStream.Seek(0, SeekOrigin.Begin);
        await using var fileStream = File.Create(path);
        await testZipStream.CopyToAsync(fileStream);
    }

    private static void CleanupTestFile()
    {
        if (File.Exists(TestArchivePath))
        {
            File.Delete(TestArchivePath);
        }
    }
}
