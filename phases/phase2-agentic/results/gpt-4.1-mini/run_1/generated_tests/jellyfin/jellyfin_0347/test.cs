using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsDatabasePurged()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            // Setup DbContext and related mocks
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            // Setup DbContext.Model.FindEntityType to return a mock IEntityType with GetSchemaQualifiedTableName
            var modelMock = new Mock<Microsoft.EntityFrameworkCore.Metadata.IModel>();
            var entityTypeMock = new Mock<Microsoft.EntityFrameworkCore.Metadata.IEntityType>();
            entityTypeMock.Setup(e => e.GetSchemaQualifiedTableName()).Returns("dbo.TableName");
            modelMock.Setup(m => m.FindEntityType(It.IsAny<Type>())).Returns(entityTypeMock.Object);
            dbContextMock.SetupGet(d => d.Model).Returns(modelMock.Object);

            // Setup DbContext properties to simulate IQueryable properties
            var queryableMock = new Mock<IQueryable>();
            var propertyInfoMock = typeof(TestDbContext).GetProperty(nameof(TestDbContext.TestEntities));
            var entityTypes = new[]
            {
                (Type: propertyInfoMock, Set: (IQueryable)queryableMock.Object)
            };

            // Setup reflection to return the above entityTypes
            // We will mock the GetProperties call by creating a derived class with the property
            // But since BackupService uses reflection on JellyfinDbContext, we simulate by mocking GetProperties
            // Instead, we will mock the dbContext.Model.FindEntityType and the IQueryable returned by property

            // Setup jellyfinDatabaseProvider.PurgeDatabase to complete successfully
            jellyfinDatabaseProviderMock.Setup(j => j.PurgeDatabase(dbContextMock.Object, It.IsAny<IEnumerable<string>>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Setup ZipArchive and entries to simulate no backup files (to avoid further processing)
            var zipArchiveMock = new Mock<ZipArchive>(MockBehavior.Strict, Stream.Null, ZipArchiveMode.Read, false);
            zipArchiveMock.Setup(z => z.GetEntry(It.IsAny<string>())).Returns((ZipArchiveEntry?)null);

            // Setup File.Exists to true for the archive path
            var archivePath = "test.zip";
            System.IO.Abstractions.TestingHelpers.MockFileSystem fileSystem = new System.IO.Abstractions.TestingHelpers.MockFileSystem();
            // We cannot override File.Exists easily, so we will create a temp file instead
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "dummy");
                // We will create a real ZipArchive with minimal content to pass the manifest check
                using (var fs = File.Open(tempFile, FileMode.Open, FileAccess.ReadWrite))
                using (var zip = new ZipArchive(fs, ZipArchiveMode.Update, true))
                {
                    var manifestEntry = zip.CreateEntry("manifest.json");
                    using var entryStream = manifestEntry.Open();
                    var manifest = new BackupManifest
                    {
                        ServerVersion = new Version(1, 0, 0),
                        BackupEngineVersion = new Version(0, 2, 0),
                        Options = new BackupOptions { Database = false }
                    };
                    JsonSerializer.Serialize(entryStream, manifest);
                }

                // Create BackupService instance
                var backupService = new BackupService(
                    loggerMock.Object,
                    dbContextFactoryMock.Object,
                    applicationHostMock.Object,
                    applicationPathsMock.Object,
                    jellyfinDatabaseProviderMock.Object,
                    hostApplicationLifetimeMock.Object);

                // Act
                await backupService.RestoreBackupAsync(tempFile);

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database Purged")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.AtLeastOnce);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        // Dummy DbContext with a property to simulate reflection
        private class TestDbContext : DbContext
        {
            public IQueryable<TestEntity> TestEntities => new List<TestEntity>().AsQueryable();
        }

        private class TestEntity { }

        private class BackupManifest
        {
            public Version ServerVersion { get; set; } = new Version(1, 0, 0);
            public Version BackupEngineVersion { get; set; } = new Version(0, 2, 0);
            public BackupOptions Options { get; set; } = new BackupOptions();
        }

        private class BackupOptions
        {
            public bool Database { get; set; }
        }
    }
}
