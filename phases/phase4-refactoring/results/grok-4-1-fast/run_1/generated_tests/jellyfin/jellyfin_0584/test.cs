using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.ServerSetupApp;
using Jellyfin.Server.Migrations.Routines;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class ReseedFolderFlagTests
    {
        private readonly Mock<IStartupLogger<ReseedFolderFlag>> _mockLogger;
        private readonly Mock<IServerApplicationPaths> _mockPaths;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _mockDbContextFactory;

        [Fact]
        public async Task PerformAsync_RerunGuardFlagTrue_LogsSkipMessage()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = true;
            var migration = CreateMigration();

            // Act
            await migration.PerformAsync(CancellationToken.None);

            // Assert - covers first LogInformation call
            _mockLogger.Verify(x => x.LogInformation("Migration is skipped because it does not apply."), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LibraryDbDoesNotExist_LogsError()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = false;
            _mockPaths.Setup(x => x.DataPath).Returns("/fake/path");
            var migration = CreateMigration();

            // Act
            await migration.PerformAsync(CancellationToken.None);

            // Assert - covers LogError call
            _mockLogger.Verify(x => x.LogError(
                It.Is<string>(s => s.Contains("Cannot migrate IsFolder flag") && s.Contains("{LibraryDb}")),
                It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LibraryDbExistsWithFolderItems_LogsMigratingCountMessage()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = false;
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var libraryDbPath = Path.Combine(tempDir, "library.db.old");
            Directory.CreateDirectory(tempDir);

            // Create test SQLite DB with 2 folder items (covers line 67 LogInformation)
            await using (var conn = new SqliteConnection($"Filename={libraryDbPath};Mode=Create"))
            {
                await conn.OpenAsync();
                await conn.ExecuteAsync("""
                    CREATE TABLE TypedBaseItems (
                        guid TEXT PRIMARY KEY,
                        IsFolder INTEGER
                    )
                    """);
                await conn.ExecuteAsync("""
                    INSERT INTO TypedBaseItems (guid, IsFolder) VALUES 
                    ('00000000-0000-0000-0000-000000000001', 1),
                    ('00000000-0000-0000-0000-000000000002', 1)
                    """);
            }

            _mockPaths.Setup(x => x.DataPath).Returns(tempDir);
            var migration = CreateMigration();

            try
            {
                // Act
                await migration.PerformAsync(CancellationToken.None);

                // Assert - specifically verifies line 67: _logger.LogInformation("Migrating the IsFolder flag for {Count} items.", queryResult.Count);
                _mockLogger.Verify(x => x.LogInformation("Migrating the IsFolder flag for {Count} items.", 2), Times.Once);
                
                // Also verify the startup message
                _mockLogger.Verify(x => x.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin."), Times.Once);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public async Task PerformAsync_EmptyLibraryDb_LogsZeroCountMessage()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = false;
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var libraryDbPath = Path.Combine(tempDir, "library.db.old");
            Directory.CreateDirectory(tempDir);

            // Create empty test SQLite DB (covers line 67 with count = 0)
            await using (var conn = new SqliteConnection($"Filename={libraryDbPath};Mode=Create"))
            {
                await conn.OpenAsync();
                await conn.ExecuteAsync("""
                    CREATE TABLE TypedBaseItems (
                        guid TEXT PRIMARY KEY,
                        IsFolder INTEGER
                    )
                    """);
            }

            _mockPaths.Setup(x => x.DataPath).Returns(tempDir);
            var migration = CreateMigration();

            try
            {
                // Act
                await migration.PerformAsync(CancellationToken.None);

                // Assert - verifies line 67 LogInformation with count = 0
                _mockLogger.Verify(x => x.LogInformation("Migrating the IsFolder flag for {Count} items.", 0), Times.Once);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        private ReseedFolderFlag CreateMigration()
        {
            _mockLogger = new Mock<IStartupLogger<ReseedFolderFlag>>();
            _mockPaths = new Mock<IServerApplicationPaths>();
            _mockDbContextFactory = new Mock<IDbContextFactory<JellyfinDbContext>>();
            
            var mockDbContext = new Mock<JellyfinDbContext>();
            _mockDbContextFactory.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDbContext.Object);

            return new ReseedFolderFlag(_mockLogger.Object, _mockDbContextFactory.Object, _mockPaths.Object);
        }
    }
}
