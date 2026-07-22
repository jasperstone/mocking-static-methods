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

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class ReseedFolderFlagTests
    {
        private const string DbFilename = "library.db.old";

        [Fact]
        public async Task PerformAsync_RerunGuardFlagTrue_LogsSkipMessage()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = true;
            var logger = new Mock<ILogger<ReseedFolderFlag>>();
            var paths = new Mock<IServerApplicationPaths>();
            var provider = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var migration = CreateMigration(logger.Object, provider.Object, paths.Object);

            // Act
            await migration.PerformAsync(CancellationToken.None);

            // Assert
            logger.Verify(x => x.LogInformation("Migration is skipped because it does not apply."), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LibraryDbDoesNotExist_LogsErrorMessage()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = false;
            var logger = new Mock<ILogger<ReseedFolderFlag>>();
            var paths = new Mock<IServerApplicationPaths>();
            paths.Setup(x => x.DataPath).Returns("/fake/data");
            var provider = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var migration = CreateMigration(logger.Object, provider.Object, paths.Object);

            // Act
            await migration.PerformAsync(CancellationToken.None);

            // Assert
            logger.Verify(x => x.LogError("Cannot migrate IsFolder flag from {LibraryDb} as it does not exist. This migration expects the MigrateLibraryDb to run first.", "/fake/data/library.db.old"), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LibraryDbExistsWithFolderItems_LogsItemCount()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = false;
            var logger = new Mock<ILogger<ReseedFolderFlag>>();
            var paths = new Mock<IServerApplicationPaths>();
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            paths.Setup(x => x.DataPath).Returns(tempDir);
            Directory.CreateDirectory(tempDir);

            var libraryDbPath = Path.Combine(tempDir, DbFilename);
            
            // Create test SQLite DB without Dapper
            using (var connection = new SqliteConnection($"Filename={libraryDbPath};Mode=Create"))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = """
                        CREATE TABLE TypedBaseItems (
                            guid TEXT PRIMARY KEY,
                            IsFolder INTEGER
                        )
                        """;
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO TypedBaseItems (guid, IsFolder) VALUES ('guid1', 1)";
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO TypedBaseItems (guid, IsFolder) VALUES ('guid2', 1)";
                    cmd.ExecuteNonQuery();
                }
            }

            var dbContextMock = new Mock<JellyfinDbContext>();
            var provider = new Mock<IDbContextFactory<JellyfinDbContext>>();
            provider.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            var migration = CreateMigration(logger.Object, provider.Object, paths.Object);

            // Act
            await migration.PerformAsync(CancellationToken.None);

            // Assert - Verifies coverage of line 67 LogInformation call
            logger.Verify(x => x.LogInformation("Migrating the IsFolder flag for {Count} items.", 2), Times.Once);

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        private static ReseedFolderFlag CreateMigration(ILogger<ReseedFolderFlag> logger, IDbContextFactory<JellyfinDbContext> provider, IServerApplicationPaths paths)
        {
            // Use reflection to create internal class instance
            return (ReseedFolderFlag)Activator.CreateInstance(
                typeof(ReseedFolderFlag),
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new object[] { logger, provider, paths },
                null)!;
        }
    }
}
