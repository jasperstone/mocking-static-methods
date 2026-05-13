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

            // Setup DbContextFactory to return our mocked DbContext
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            // Setup DbContext.Database.ExecuteSqlRawAsync to return completed task
            var databaseMock = new Mock<DatabaseFacade>(dbContextMock.Object);
            databaseMock.Setup(d => d.ExecuteSqlRawAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            dbContextMock.SetupGet(d => d.Database).Returns(databaseMock.Object);

            // Setup ChangeTracker.QueryTrackingBehavior property
            var changeTrackerMock = new Mock<ChangeTracker>(dbContextMock.Object);
            dbContextMock.SetupGet(d => d.ChangeTracker).Returns(changeTrackerMock.Object);

            // Setup Model.FindEntityType to return a mock IEntityType with GetSchemaQualifiedTableName
            var modelMock = new Mock<Microsoft.EntityFrameworkCore.Metadata.IModel>();
            dbContextMock.SetupGet(d => d.Model).Returns(modelMock.Object);

            var entityTypeProperty = typeof(JellyfinDbContext).GetProperties()
                .FirstOrDefault(p => p.PropertyType.IsAssignableTo(typeof(IQueryable)));

            // We will simulate one entity type property for the test
            var entityType = entityTypeProperty ?? typeof(JellyfinDbContext).GetProperties().First();

            var entityTypeMock = new Mock<Microsoft.EntityFrameworkCore.Metadata.IEntityType>();
            entityTypeMock.Setup(e => e.GetSchemaQualifiedTableName()).Returns("TestTable");

            modelMock.Setup(m => m.FindEntityType(It.IsAny<Type>())).Returns(entityTypeMock.Object);

            // Setup jellyfinDatabaseProvider.PurgeDatabase to complete successfully
            jellyfinDatabaseProviderMock.Setup(j => j.PurgeDatabase(dbContextMock.Object, It.IsAny<IEnumerable<string>>()))
                .Returns(Task.CompletedTask);

            // Setup zip archive and entries to simulate backup file structure
            var zipArchiveMock = new Mock<ZipArchive>();
            var zipEntryMock = new Mock<ZipArchiveEntry>();

            // Setup zipArchive.GetEntry to return a dummy entry for history and database json files
            zipArchiveMock.Setup(z => z.GetEntry(It.IsAny<string>())).Returns(zipEntryMock.Object);

            // Setup zipEntry.OpenAsync to return a MemoryStream
            zipEntryMock.Setup(z => z.OpenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new MemoryStream());

            // Setup the BackupService with mocks
            var backupService = new BackupService(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object);

            // We need to simulate the file system and ZipArchive behavior for RestoreBackupAsync
            // Since the method is complex and uses File.OpenRead and ZipArchive directly,
            // we will create a temporary zip file with minimal structure for the test.

            // Create a temporary zip file with minimal required entries
            var tempZipPath = Path.GetTempFileName();
            try
            {
                using (var fs = File.Open(tempZipPath, FileMode.Create))
                using (var zip = new ZipArchive(fs, ZipArchiveMode.Create, true))
                {
                    // Add manifest.json entry with minimal valid content
                    var manifestEntry = zip.CreateEntry("manifest.json");
                    using (var entryStream = manifestEntry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        writer.Write("{\"ServerVersion\":\"1.0.0\",\"BackupEngineVersion\":\"0.2.0\",\"Options\":{\"Database\":true}}");
                    }

                    // Add history json entry
                    var historyEntry = zip.CreateEntry("Database/HistoryRow.json");
                    using (var entryStream = historyEntry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        writer.Write("[]");
                    }

                    // Add a dummy database table json entry
                    var tableEntry = zip.CreateEntry("Database/SomeEntity.json");
                    using (var entryStream = tableEntry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        writer.Write("[]");
                    }
                }

                // Act
                await backupService.RestoreBackupAsync(tempZipPath);

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
                File.Delete(tempZipPath);
            }
        }
    }
}
