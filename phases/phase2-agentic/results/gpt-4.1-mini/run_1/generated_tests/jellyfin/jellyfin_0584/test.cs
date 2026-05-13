using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class ReseedFolderFlagTests
    {
        private readonly Mock<IStartupLogger> _loggerMock;
        private readonly Mock<IServerApplicationPaths> _pathsMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbContextFactoryMock;
        private readonly Mock<JellyfinDbContext> _dbContextMock;

        public ReseedFolderFlagTests()
        {
            _loggerMock = new Mock<IStartupLogger>();
            _pathsMock = new Mock<IServerApplicationPaths>();
            _dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _dbContextMock = new Mock<JellyfinDbContext>();

            // Setup DbContext BaseItems property to return a mock DbSet
            var baseItemsMock = new Mock<DbSet<BaseItem>>();
            _dbContextMock.Setup(db => db.BaseItems).Returns(baseItemsMock.Object);

            // Setup CreateDbContextAsync to return the mocked DbContext
            _dbContextFactoryMock.Setup(factory => factory.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_dbContextMock.Object);
        }

        [Fact]
        public async Task PerformAsync_WhenRerunGuardFlagIsTrue_LogsSkipAndReturns()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = true;
            var routine = new ReseedFolderFlag(_loggerMock.Object, _dbContextFactoryMock.Object, _pathsMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Migration is skipped because it does not apply."), Times.Once);
            // Reset flag for other tests
            ReseedFolderFlag.RerunGuardFlag = false;
        }

        [Fact]
        public async Task PerformAsync_WhenLibraryDbDoesNotExist_LogsErrorAndReturns()
        {
            // Arrange
            var dataPath = Path.GetTempPath();
            _pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            var routine = new ReseedFolderFlag(_loggerMock.Object, _dbContextFactoryMock.Object, _pathsMock.Object);

            // Ensure the file does not exist
            var libraryDbPath = Path.Combine(dataPath, "library.db.old");
            if (File.Exists(libraryDbPath))
            {
                File.Delete(libraryDbPath);
            }

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogError(
                It.Is<string>(s => s.Contains("Cannot migrate IsFolder flag from")),
                It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LogsInformationWithCount_WhenQueryReturnsResults()
        {
            // Arrange
            var dataPath = Path.GetTempPath();
            _pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            var libraryDbPath = Path.Combine(dataPath, "library.db.old");

            // Create a temporary SQLite file with the expected table and data
            using (var connection = new SqliteConnection($"Filename={libraryDbPath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS TypedBaseItems (guid TEXT, IsFolder BOOLEAN);
                    DELETE FROM TypedBaseItems;
                    INSERT INTO TypedBaseItems (guid, IsFolder) VALUES ('00000000-0000-0000-0000-000000000001', 1);
                    INSERT INTO TypedBaseItems (guid, IsFolder) VALUES ('00000000-0000-0000-0000-000000000002', 1);
                ";
                command.ExecuteNonQuery();
            }

            var routine = new ReseedFolderFlag(_loggerMock.Object, _dbContextFactoryMock.Object, _pathsMock.Object);

            // Setup BaseItems.Where(...).ExecuteUpdateAsync(...) to simulate update
            var baseItemsMock = new Mock<DbSet<BaseItem>>();
            var queryable = new List<BaseItem>().AsQueryable();
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.Provider).Returns(queryable.Provider);
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.Expression).Returns(queryable.Expression);
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            baseItemsMock.As<IQueryable<BaseItem>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            _dbContextMock.Setup(db => db.BaseItems).Returns(baseItemsMock.Object);

            // Setup ExecuteUpdateAsync extension method mock by intercepting the call
            // Since ExecuteUpdateAsync is an extension method, we cannot mock it directly.
            // We will just verify that the method completes without exceptions.

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogInformation(
                "Migrating the IsFolder flag for {Count} items.",
                It.Is<int>(count => count == 2)), Times.Once);

            // Cleanup
            File.Delete(libraryDbPath);
        }
    }
}
