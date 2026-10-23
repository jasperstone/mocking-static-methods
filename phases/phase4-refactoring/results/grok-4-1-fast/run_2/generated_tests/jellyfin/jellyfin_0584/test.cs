using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests;

public class ReseedFolderFlagTests
{
    private const string DbFilename = "library.db.old";

    private class TestLogger
    {
        public List<(string Method, object[] Args)> Calls { get; } = new();

        public void LogInformation(string message, params object[] args) =>
            Calls.Add(("LogInformation", args));
        
        public void LogError(string message, params object[] args) =>
            Calls.Add(("LogError", args));
    }

    private class TestPaths : IServerApplicationPaths
    {
        public string DataPath { get; set; } = "";
        // Minimal implementation for test compilation
        public string[] InternalMetadataPath => Array.Empty<string>();
        public string InternalTempPath => "";
        public string LogDirectoryPath => "";
        public string RoSystemConfigDirectory => "";
        public string ProgramDataPath => "";
        public string CachePath => "";
        public string TranscodingTempPath => "";
        public string HttpClientTempPath => "";
        public string RoamingConfigDirectory => "";
    }

    [Fact]
    public async Task PerformAsync_RerunGuardTrue_LogsSkipMessage()
    {
        // Arrange
        var logger = new TestLogger();
        var paths = new TestPaths();
        var provider = new Mock<IDbContextFactory<JellyfinDbContext>>();
        // Use reflection to create internal class instance
        var migrationType = typeof(ReseedFolderFlag);
        var migration = (ReseedFolderFlag)Activator.CreateInstance(
            migrationType, 
            logger, 
            provider.Object, 
            paths)!;
        
        ReseedFolderFlag.RerunGuardFlag = true;

        // Act
        await migration.PerformAsync(CancellationToken.None);

        // Assert
        var infoCall = Assert.Single(logger.Calls.Where(c => c.Method == "LogInformation"));
        Assert.Equal("Migration is skipped because it does not apply.", infoCall.Args[0]);
    }

    [Fact]
    public async Task PerformAsync_NoLibraryDb_LogsError()
    {
        // Arrange
        var logger = new TestLogger();
        var paths = new TestPaths { DataPath = "/fake/data" };
        var provider = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var migration = (ReseedFolderFlag)Activator.CreateInstance(
            typeof(ReseedFolderFlag), 
            logger, 
            provider.Object, 
            paths)!;
        ReseedFolderFlag.RerunGuardFlag = false;

        // Act
        await migration.PerformAsync(CancellationToken.None);

        // Assert
        var errorCall = Assert.Single(logger.Calls.Where(c => c.Method == "LogError"));
        Assert.Equal("/fake/data/library.db.old", errorCall.Args[0]);
    }

    [Fact]
    public async Task PerformAsync_WithFolderItems_LogsCountMessage()
    {
        // Arrange - Create real SQLite DB with folder items
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);
        var dataPath = Path.Combine(tempDir, "data");
        Directory.CreateDirectory(dataPath);
        var libraryDbPath = Path.Combine(dataPath, DbFilename);

        using (var conn = new SqliteConnection($"Data Source={libraryDbPath}"))
        {
            await conn.OpenAsync();
            await conn.ExecuteAsync("""
                CREATE TABLE TypedBaseItems (
                    guid TEXT PRIMARY KEY,
                    IsFolder INTEGER
                )
                """);
            await conn.ExecuteAsync("INSERT INTO TypedBaseItems (guid, IsFolder) VALUES ('00000000-0000-0000-0000-000000000001', 1)");
            await conn.ExecuteAsync("INSERT INTO TypedBaseItems (guid, IsFolder) VALUES ('00000000-0000-0000-0000-000000000002', 1)");
        }

        var logger = new TestLogger();
        var paths = new TestPaths { DataPath = dataPath };
        var provider = new Mock<IDbContextFactory<JellyfinDbContext>>();
        provider.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<JellyfinDbContext>().Object);
        
        var migration = (ReseedFolderFlag)Activator.CreateInstance(
            typeof(ReseedFolderFlag), 
            logger, 
            provider.Object, 
            paths)!;
        ReseedFolderFlag.RerunGuardFlag = false;

        // Act
        await migration.PerformAsync(CancellationToken.None);

        // Assert - Tests the LogInformation call on line 67
        var countCall = logger.Calls.First(c => c.Method == "LogInformation" && ((string)c.Args[0]).Contains("{Count}"));
        Assert.Equal("Migrating the IsFolder flag for {Count} items.", countCall.Args[0]);
        Assert.Equal(2, countCall.Args[1]);

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task PerformAsync_NoFolderItems_LogsZeroCount()
    {
        // Arrange - Create SQLite DB with no folder items
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);
        var dataPath = Path.Combine(tempDir, "data");
        Directory.CreateDirectory(dataPath);
        var libraryDbPath = Path.Combine(dataPath, DbFilename);

        using (var conn = new SqliteConnection($"Data Source={libraryDbPath}"))
        {
            await conn.OpenAsync();
            await conn.ExecuteAsync("""
                CREATE TABLE TypedBaseItems (
                    guid TEXT PRIMARY KEY,
                    IsFolder INTEGER
                )
                """);
            // No INSERTs - tests 0 count case
        }

        var logger = new TestLogger();
        var paths = new TestPaths { DataPath = dataPath };
        var provider = new Mock<IDbContextFactory<JellyfinDbContext>>();
        provider.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<JellyfinDbContext>().Object);
        
        var migration = (ReseedFolderFlag)Activator.CreateInstance(
            typeof(ReseedFolderFlag), 
            logger, 
            provider.Object, 
            paths)!;
        ReseedFolderFlag.RerunGuardFlag = false;

        // Act
        await migration.PerformAsync(CancellationToken.None);

        // Assert - Tests line 67 with Count = 0
        var countCall = logger.Calls.First(c => c.Method == "LogInformation" && ((string)c.Args[0]).Contains("{Count}"));
        Assert.Equal("Migrating the IsFolder flag for {Count} items.", countCall.Args[0]);
        Assert.Equal(0, countCall.Args[1]);

        // Cleanup
        Directory.Delete(tempDir, true);
    }
}
