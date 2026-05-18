using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Data;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.ServerSetupApp;
using MediaBrowser.Controller;
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
        public async Task PerformAsync_LogsInformationForMigratingItems()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var dataPath = "test_data_path";
            var libraryDbPath = Path.Combine(dataPath, "library.db.old");

            pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            fileSystemMock.Setup(fs => fs.Exists(libraryDbPath)).Returns(true);

            var connectionMock = new Mock<SqliteConnection>();
            var queryResult = new List<SqliteDataReader>
            {
                Mock.Of<SqliteDataReader>(reader =>
                    reader.GetGuid(0) == Guid.NewGuid())
            };
            connectionMock.Setup(c => c.Query(It.IsAny<string>())).Returns(queryResult);

            var dbContextMock = new Mock<JellyfinDbContext>();
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            var sut = new ReseedFolderFlag(
                new Mock<IStartupLogger<ReseedLibraryDb>>().Object,
                dbContextFactoryMock.Object,
                pathsMock.Object, fileSystemMock.Object);

            // Act
            await sut.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation("Migrating the IsFolder flag for {Count} items.", It.Is<int>(count => count == queryResult.Count)),
                Times.Once);
        }
    }

    public interface IFileSystem
    {
        bool Exists(string path);
    }

    public class ReseedFolderFlag : IAsyncMigrationRoutine
    {
        private const string DbFilename = "library.db.old";

        private readonly IStartupLogger _logger;
        private readonly IServerApplicationPaths _paths;
        private readonly IDbContextFactory<JellyfinDbContext> _provider;
        private readonly IFileSystem _fileSystem;

        public ReseedFolderFlag(
            IStartupLogger<MigrateLibraryDb> startupLogger,
            IDbContextFactory<JellyfinDbContext> provider,
            IServerApplicationPaths paths,
            IFileSystem fileSystem)
        {
            _logger = startupLogger;
            _provider = provider;
            _paths = paths;
            _fileSystem = fileSystem;
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
            if (!_fileSystem.Exists(libraryDbPath))
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
}
