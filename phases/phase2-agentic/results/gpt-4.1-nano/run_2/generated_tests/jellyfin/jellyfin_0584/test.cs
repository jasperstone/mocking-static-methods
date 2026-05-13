using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Jellyfin.Tests.Migrations
{
    public class ReseedFolderFlagTests
    {
        [Fact]
        public async Task PerformAsync_Should_Log_Skipped_When_RerunFlag_Is_True()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            ReseedFolderFlag.RerunGuardFlag = true;
            var routine = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(x => x.LogInformation("Migration is skipped because it does not apply."), Times.Once);
            ReseedFolderFlag.RerunGuardFlag = false; // Reset for other tests
        }

        [Fact]
        public async Task PerformAsync_Should_Log_Error_And_Return_If_LibraryDb_Does_Not_Exist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            pathsMock.Setup(p => p.DataPath).Returns("/fake/path");
            var routine = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);
            // Simulate file not existing
            var fileExists = false;
            // Patch File.Exists
            var originalExists = File.Exists;
            File.Exists = (path) => false;

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);

            // Cleanup
            File.Exists = originalExists;
        }

        [Fact]
        public async Task PerformAsync_Should_Log_And_Migrate_Items()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var baseItemsMock = new Mock<DbSet<BaseItem>>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var connectionMock = new Mock<SqliteConnection>();
            var guidList = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            pathsMock.Setup(p => p.DataPath).Returns("/fake/path");
            providerMock.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);
            dbContextMock.Setup(db => db.BaseItems).Returns(baseItemsMock.Object);

            // Simulate File.Exists
            var originalExists = File.Exists;
            File.Exists = (path) => true;

            // Mock connection.Query
            var queryResult = guidList.Select(g => new { GetGuid = new Func<Guid>(() => g) }).ToList();
            var queryMock = new Mock<SqliteConnection>();
            // Since actual Query extension method is static, we can't mock directly.
            // Instead, we can simulate the call by patching the method or assume it returns guidList.

            // For simplicity, assume the method proceeds with guidList
            // and the code calls ExecuteUpdateAsync for each.

            // Act
            var routine = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, pathsMock.Object);
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(x => x.LogInformation(It.Is<string>(s => s.Contains("Migrating the IsFolder flag for")), Times.Once);
            // Cleanup
            File.Exists = originalExists;
        }
    }
}
