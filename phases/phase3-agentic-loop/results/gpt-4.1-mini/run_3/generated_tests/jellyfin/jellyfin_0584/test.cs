using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class ReseedFolderFlagTests
    {
        private class DummyEntity
        {
            public Guid Id { get; set; }
            public bool IsFolder { get; set; }
        }

        private class DummyDbContext : DbContext
        {
            public DummyDbContext(DbContextOptions options) : base(options) { }

            public DbSet<DummyEntity> BaseItems { get; set; }
        }

        private class DummyServerApplicationPaths : Jellyfin.Server.ServerSetupApp.IServerApplicationPaths
        {
            public string DataPath { get; set; }
        }

        [Fact]
        public async Task PerformAsync_LogsInformationOnRerunGuardFlag()
        {
            // Arrange
            var loggerMock = new Mock<Jellyfin.Server.ServerSetupApp.IStartupLogger<MigrateLibraryDb>>();
            var providerMock = new Mock<Microsoft.EntityFrameworkCore.IDbContextFactory<DummyDbContext>>();
            var paths = new DummyServerApplicationPaths { DataPath = Path.GetTempPath() };

            ReseedFolderFlag.RerunGuardFlag = true;

            var routine = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, paths);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Migration is skipped because it does not apply."), Times.Once);
            ReseedFolderFlag.RerunGuardFlag = false;
        }

        [Fact]
        public async Task PerformAsync_LogsInformationForCountItems()
        {
            // Arrange
            var loggerMock = new Mock<Jellyfin.Server.ServerSetupApp.IStartupLogger<MigrateLibraryDb>>();
            var paths = new DummyServerApplicationPaths { DataPath = Path.GetTempPath() };

            // Create a temporary file to simulate library.db.old presence
            var tempFile = Path.Combine(paths.DataPath, "library.db.old");
            File.WriteAllText(tempFile, "dummy content");

            var dummyEntities = new List<DummyEntity>
            {
                new DummyEntity { Id = Guid.NewGuid(), IsFolder = true },
                new DummyEntity { Id = Guid.NewGuid(), IsFolder = true }
            };

            var options = new DbContextOptionsBuilder<DummyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dbContext = new DummyDbContext(options);
            dbContext.BaseItems.AddRange(dummyEntities);
            await dbContext.SaveChangesAsync();

            var providerMock = new Mock<Microsoft.EntityFrameworkCore.IDbContextFactory<DummyDbContext>>();
            providerMock.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContext);

            var routine = new ReseedFolderFlag(loggerMock.Object, providerMock.Object, paths);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Migrating the IsFolder flag for")), It.IsAny<object[]>()), Times.Once);

            // Cleanup
            File.Delete(tempFile);
        }
    }
}
