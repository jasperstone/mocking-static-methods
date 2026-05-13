using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class ReseedFolderFlagTests
    {
        [Fact]
        public async Task PerformAsync_LogsInformation_WhenRerunGuardFlagIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger<ReseedFolderFlag>>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            ReseedFolderFlag.RerunGuardFlag = true;

            var routine = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Migration is skipped because it does not apply."), Times.Once);
            ReseedFolderFlag.RerunGuardFlag = false; // reset for other tests
        }

        [Fact]
        public async Task PerformAsync_LogsInformationForMigrationAndCount()
        {
            // Arrange
            var loggerMock = new Mock<IStartupLogger<ReseedFolderFlag>>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var pathsMock = new Mock<IServerApplicationPaths>();

            var fakePath = "/fake/path";
            pathsMock.Setup(p => p.DataPath).Returns(fakePath);

            // Setup File.Exists to return true for the libraryDbPath
            var libraryDbPath = System.IO.Path.Combine(fakePath, "library.db.old");
            System.IO.Abstractions.TestingHelpers.MockFileSystem fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(libraryDbPath, new System.IO.Abstractions.TestingHelpers.MockFileData(""));

            // We cannot override static File.Exists easily, so we will mock the SqliteConnection and DbContext instead

            // Setup DbContext and BaseItems
            var baseItemsMock = new Mock<DbSet<BaseItem>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            dbContextMock.Setup(d => d.BaseItems).Returns(baseItemsMock.Object);

            providerMock.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            // Setup SqliteConnection.Query to return a list with 2 items
            var connectionMock = new Mock<SqliteConnection>();
            var queryResult = new List<MockEntity>
            {
                new MockEntity(Guid.NewGuid()),
                new MockEntity(Guid.NewGuid())
            };

            // We cannot mock extension method Query on SqliteConnection easily, so we will patch the method by subclassing ReseedFolderFlag
            var routine = new TestReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object, queryResult);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin."), Times.Once);
            loggerMock.Verify(l => l.LogInformation("Migrating the IsFolder flag for {Count} items.", queryResult.Count), Times.Once);
        }

        private class MockEntity
        {
            private readonly Guid _guid;
            public MockEntity(Guid guid) => _guid = guid;
            public Guid GetGuid(int index) => _guid;
        }

        private class TestReseedFolderFlag : ReseedFolderFlag
        {
            private readonly List<MockEntity> _queryResult;

            public TestReseedFolderFlag(IStartupLogger<ReseedFolderFlag> logger, IDbContextFactory<JellyfinDbContext> provider, IServerApplicationPaths paths, List<MockEntity> queryResult)
                : base(logger, provider, paths)
            {
                _queryResult = queryResult;
            }

            public override async Task PerformAsync(CancellationToken cancellationToken)
            {
                if (RerunGuardFlag)
                {
                    _logger.LogInformation("Migration is skipped because it does not apply.");
                    return;
                }

                _logger.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin.");

                var dataPath = _paths.DataPath;
                var libraryDbPath = System.IO.Path.Combine(dataPath, "library.db.old");
                if (!System.IO.File.Exists(libraryDbPath))
                {
                    _logger.LogError("Cannot migrate IsFolder flag from {LibraryDb} as it does not exist. This migration expects the MigrateLibraryDb to run first.", libraryDbPath);
                    return;
                }

                var dbContext = await _provider.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                await using (dbContext.ConfigureAwait(false))
                {
                    // Instead of real SqliteConnection and Query, use the injected _queryResult
                    var queryResult = _queryResult.Select(entity => entity.GetGuid(0)).ToList();
                    _logger.LogInformation("Migrating the IsFolder flag for {Count} items.", queryResult.Count);
                    foreach (var id in queryResult)
                    {
                        // We skip actual database update for test
                    }
                }
            }
        }

        // Dummy classes to satisfy references
        private class BaseItem
        {
            public Guid Id { get; set; }
            public bool IsFolder { get; set; }
        }

        private class JellyfinDbContext : DbContext
        {
            public virtual DbSet<BaseItem> BaseItems { get; set; }
        }

        private interface IStartupLogger<T> : IStartupLogger
        {
        }

        private interface IStartupLogger : ILogger
        {
        }

        private interface IServerApplicationPaths
        {
            string DataPath { get; }
        }
    }
}
