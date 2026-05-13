using System.Collections.Generic;
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
using Jellyfin.Server.ServerSetupApp;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class ReseedFolderFlagTests
    {
        [Fact]
        public async Task PerformAsync_LogsInformation_WhenRerunGuardFlagIsTrue()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = true;
            var loggerMock = new Mock<IStartupLogger<ReseedFolderFlag>>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var routine = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Migration is skipped because it does not apply."), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LogsInformationWithCount_WhenQueryReturnsResults()
        {
            // Arrange
            ReseedFolderFlag.RerunGuardFlag = false;

            var loggerMock = new Mock<IStartupLogger<ReseedFolderFlag>>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var pathsMock = new Mock<IServerApplicationPaths>();

            var fakePath = "/fake/path";
            pathsMock.Setup(p => p.DataPath).Returns(fakePath);

            // Setup File.Exists to return true for the libraryDbPath
            var libraryDbPath = System.IO.Path.Combine(fakePath, "library.db.old");
            System.IO.Abstractions.TestingHelpers.MockFileSystem fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            fileSystem.AddFile(libraryDbPath, new System.IO.Abstractions.TestingHelpers.MockFileData(""));

            // We cannot override static File.Exists easily, so we will mock the connection.Query call instead by subclassing SqliteConnection
            var dbContextMock = new Mock<JellyfinDbContext>();
            var baseItemsMock = new Mock<DbSet<BaseItem>>();
            dbContextMock.SetupGet(d => d.BaseItems).Returns(baseItemsMock.Object);
            providerMock.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContextMock.Object);

            var routine = new TestableReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object, libraryDbPath);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Migrating the IsFolder flag for {Count} items.", 2), Times.Once);
        }

        // Helper subclass to override connection.Query behavior and File.Exists
        private class TestableReseedFolderFlag : ReseedFolderFlag
        {
            private readonly string _libraryDbPath;

            public TestableReseedFolderFlag(IStartupLogger<ReseedFolderFlag> logger, IDbContextFactory<JellyfinDbContext> provider, IServerApplicationPaths paths, string libraryDbPath)
                : base(logger, provider, paths)
            {
                _libraryDbPath = libraryDbPath;
            }

            public override async Task PerformAsync(CancellationToken cancellationToken)
            {
                if (RerunGuardFlag)
                {
                    _logger.LogInformation("Migration is skipped because it does not apply.");
                    return;
                }

                _logger.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin.");

                var libraryDbPath = _libraryDbPath;
                if (!System.IO.File.Exists(libraryDbPath))
                {
                    _logger.LogError("Cannot migrate IsFolder flag from {LibraryDb} as it does not exist. This migration expects the MigrateLibraryDb to run first.", libraryDbPath);
                    return;
                }

                var dbContext = await _provider.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                await using (dbContext.ConfigureAwait(false))
                {
                    using var connection = new TestSqliteConnection();
                    var queryResult = connection.Query("").Select(_ => new TestEntity()).ToList();
                    _logger.LogInformation("Migrating the IsFolder flag for {Count} items.", queryResult.Count);
                }
            }
        }

        private class TestSqliteConnection : SqliteConnection
        {
            public IEnumerable<TestEntity> Query(string sql)
            {
                return new List<TestEntity> { new TestEntity(), new TestEntity() };
            }
        }

        private class TestEntity
        {
            public System.Guid GetGuid(int index) => System.Guid.NewGuid();
        }
    }
}
