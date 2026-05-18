using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Data;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.ServerSetupApp;
using Jellyfin.Server.Migrations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class ReseedFolderFlagTests
    {
        [Fact]
        public async Task PerformAsync_LogsCorrectInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();

            var dataPath = "test_data_path";
            var libraryDbPath = Path.Combine(dataPath, "library.db.old");
            var queryResult = new List<object> { new { GetGuid = (Func<int, Guid>)((index) => Guid.NewGuid()) } };

            pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            providerMock.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new JellyfinDbContext(new DbContextOptions<JellyfinDbContext>()));

            using var connection = new SqliteConnection($"Filename={libraryDbPath};Mode=Memory;");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE TypedBaseItems (guid TEXT, IsFolder BOOLEAN);";
            command.ExecuteNonQuery();
            command.CommandText = "INSERT INTO TypedBaseItems (guid, IsFolder) VALUES ('test-guid', 1);";
            command.ExecuteNonQuery();

            var sut = new ReseedFolderFlag(
                new Mock<IStartupLogger<ReseedFolderFlag>>().Object,
                providerMock.Object,
                pathsMock.Object)
            {
                _logger = loggerMock.Object
            };

            // Act
            await sut.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.Is<string>(s => s.Contains("Migrating the IsFolder flag for {Count} items.")),
                    It.Is<object[]>(o => o.Length == 1 && o[0] is int count && count == 1)),
                Times.Once);
        }
    }
}

// Adjusted class with public access
public class ReseedFolderFlag : IAsyncMigrationRoutine
{
    private const string DbFilename = "library.db.old";

    private readonly IStartupLogger _logger;
    private readonly IServerApplicationPaths _paths;
    private readonly IDbContextFactory<JellyfinDbContext> _provider;

    public ReseedFolderFlag(
            IStartupLogger<MigrateLibraryDb> startupLogger,
            IDbContextFactory<JellyfinDbContext> provider,
            IServerApplicationPaths paths)
    {
        _logger = startupLogger;
        _provider = provider;
        _paths = paths;
    }

    internal static bool RerunGuardFlag { get; set; } = false;

    public async Task PerformAsync(CancellationToken cancellationToken)
    {
        if (RerunGuardFlag)
        {
            _logger.LogInformation("Migration is skipped because it does not apply.");
            return;
        }

        _logger.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin.");

        var dataPath = _paths.DataPath;
        var libraryDbPath = Path.Combine(dataPath, DbFilename);
        if (!File.Exists(libraryDbPath))
        {
            _logger.LogError("Cannot migrate IsFolder flag from {LibraryDb} as it does not exist. This migration expects the MigrateLibraryDb to run first.", libraryDbPath);
            return;
        }

        var dbContext = await _provider.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using (dbContext.ConfigureAwait(false))
        {
            using var connection = new SqliteConnection($"Filename={libraryDbPath};Mode=ReadOnly");
            var queryResult = connection.Query(
                """
                    SELECT guid FROM TypedBaseItems
                    WHERE IsFolder = true
                """)
                        .Select(entity => entity.GetGuid(0))
                        .ToList();
            _logger.LogInformation("Migrating the IsFolder flag for {Count} items.", queryResult.Count);
            foreach (var id in queryResult)
            {
                await dbContext.BaseItems.Where(e => e.Id == id).ExecuteUpdateAsync(e => e.SetProperty(f => f.IsFolder, true), cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
