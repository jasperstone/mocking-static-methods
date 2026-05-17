using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines
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

            _dbContextFactoryMock
                .Setup(factory => factory.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_dbContextMock.Object);
        }

        [Fact]
        public async Task PerformAsync_LogsSkipped_WhenRerunGuardFlagIsTrue()
        {
            SetRerunGuardFlag(true);
            var sut = CreateSut();

            await CallPerformAsync(sut);

            _loggerMock.Verify(l => l.LogInformation("Migration is skipped because it does not apply."), Times.Once);

            SetRerunGuardFlag(false);
        }

        [Fact]
        public async Task PerformAsync_LogsInfoAndError_WhenLibraryDbDoesNotExist()
        {
            SetRerunGuardFlag(false);

            var fakeDataPath = Path.GetTempPath();
            _pathsMock.Setup(p => p.DataPath).Returns(fakeDataPath);

            var sut = CreateSut();

            var libraryDbPath = Path.Combine(fakeDataPath, "library.db.old");
            if (File.Exists(libraryDbPath))
            {
                File.Delete(libraryDbPath);
            }

            await CallPerformAsync(sut);

            _loggerMock.Verify(l => l.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin."), Times.Once);
            _loggerMock.Verify(l => l.LogError(
                "Cannot migrate IsFolder flag from {LibraryDb} as it does not exist. This migration expects the MigrateLibraryDb to run first.",
                libraryDbPath), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LogsCount_WhenQueryReturnsResults()
        {
            SetRerunGuardFlag(false);

            var fakeDataPath = Path.GetTempPath();
            _pathsMock.Setup(p => p.DataPath).Returns(fakeDataPath);

            var libraryDbPath = Path.Combine(fakeDataPath, "library.db.old");

            using (var connection = new SqliteConnection($"Filename={libraryDbPath}"))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText =
                    @"CREATE TABLE IF NOT EXISTS TypedBaseItems (guid TEXT PRIMARY KEY, IsFolder BOOLEAN);
                      INSERT INTO TypedBaseItems (guid, IsFolder) VALUES ('00000000-0000-0000-0000-000000000001', 1);
                      INSERT INTO TypedBaseItems (guid, IsFolder) VALUES ('00000000-0000-0000-0000-000000000002', 1);";
                await command.ExecuteNonQueryAsync();
            }

            var baseItemsMock = new Mock<DbSet<BaseItem>>();
            _dbContextMock.SetupGet(db => db.BaseItems).Returns(baseItemsMock.Object);

            baseItemsMock.Setup(b => b.Where(It.IsAny<Func<BaseItem, bool>>()))
                .Returns(baseItemsMock.Object);

            baseItemsMock.Setup(b => b.ExecuteUpdateAsync(
                It.IsAny<Action<Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder<BaseItem>>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var sut = CreateSut();

            await CallPerformAsync(sut);

            _loggerMock.Verify(l => l.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin."), Times.Once);
            _loggerMock.Verify(l => l.LogInformation("Migrating the IsFolder flag for {Count} items.", 2), Times.Once);

            File.Delete(libraryDbPath);
        }

        private ReseedFolderFlag CreateSut()
        {
            var ctor = typeof(ReseedFolderFlag).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)[0];
            var startupLoggerType = typeof(IStartupLogger<>).MakeGenericType(typeof(MigrateLibraryDb));
            var logger = _loggerMock.As(startupLoggerType).Object;
            return (ReseedFolderFlag)ctor.Invoke(new object[] { logger, _dbContextFactoryMock.Object, _pathsMock.Object });
        }

        private Task CallPerformAsync(ReseedFolderFlag sut)
        {
            var method = typeof(ReseedFolderFlag).GetMethod("PerformAsync", BindingFlags.Instance | BindingFlags.Public);
            return (Task)method.Invoke(sut, new object[] { CancellationToken.None });
        }

        private void SetRerunGuardFlag(bool value)
        {
            var prop = typeof(ReseedFolderFlag).GetProperty("RerunGuardFlag", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            prop.SetValue(null, value);
        }
    }

    // Minimal stub classes to satisfy compilation
    public class JellyfinDbContext : DbContext
    {
        public virtual DbSet<BaseItem> BaseItems { get; set; }
    }

    public class BaseItem
    {
        public Guid Id { get; set; }
        public bool IsFolder { get; set; }
    }

    public interface IStartupLogger : ILogger
    {
    }

    public interface IServerApplicationPaths
    {
        string DataPath { get; }
    }

    public class MigrateLibraryDb { }
}
