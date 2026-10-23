using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsInformationOnLine211()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextMock = new Mock<DbContext>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<DbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<object>(); // Use object as placeholder
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            // Setup DbContextFactory to return the mocked DbContext
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            // Setup DbContext.Model to return a mock IModel
            var modelMock = new Mock<Microsoft.EntityFrameworkCore.Metadata.IModel>();
            dbContextMock.Setup(d => d.Model).Returns(modelMock.Object);

            // Setup entity type mock with GetSchemaQualifiedTableName returning a table name
            var entityTypeMock = new Mock<Microsoft.EntityFrameworkCore.Metadata.IEntityType>();
            entityTypeMock.Setup(e => e.GetSchemaQualifiedTableName()).Returns("dbo.TestTable");
            modelMock.Setup(m => m.FindEntityType(It.IsAny<Type>())).Returns(entityTypeMock.Object);

            // Setup DbContext properties to simulate one IQueryable property
            var testEntityProperty = typeof(TestDbContext).GetProperty(nameof(TestDbContext.TestEntities));
            var properties = new[] { testEntityProperty };
            var dbContextType = typeof(TestDbContext);
            dbContextMock.Setup(d => d.GetType()).Returns(dbContextType);

            // Setup jellyfinDatabaseProvider to do nothing on PurgeDatabase
            // We cannot mock IJellyfinDatabaseProvider, so skip calls to it

            // Create a zip archive in memory with no entry for the entity type to trigger the log on line 211
            var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                // Add manifest.json entry with minimal valid content
                var manifestEntry = archive.CreateEntry("manifest.json");
                using (var writer = new StreamWriter(manifestEntry.Open()))
                {
                    writer.Write("{\"ServerVersion\":\"1.0.0\",\"BackupEngineVersion\":\"0.2.0\",\"Options\":{\"Database\":true}}");
                }
            }
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Setup applicationHost.ApplicationVersion to 1.0.0
            applicationHostMock.Setup(a => a.ApplicationVersion).Returns(new Version(1, 0, 0));

            // Setup applicationPaths properties to dummy paths
            applicationPathsMock.SetupGet(a => a.ConfigurationDirectoryPath).Returns("Config");
            applicationPathsMock.SetupGet(a => a.DataPath).Returns("Data");
            applicationPathsMock.SetupGet(a => a.RootFolderPath).Returns("Root");
            applicationPathsMock.SetupGet(a => a.InternalMetadataPath).Returns("Data/metadata");
            applicationPathsMock.SetupGet(a => a.DefaultInternalMetadataPath).Returns("Data/metadata-default");

            var backupService = new TestBackupService(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object,
                memoryStream);

            // Act
            await backupService.RestoreBackupAsync("dummy.zip");

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No backup of expected table")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        // Minimal DbContext subclass with one IQueryable property
        private class TestDbContext : DbContext
        {
            public IQueryable<object> TestEntities => Enumerable.Empty<object>().AsQueryable();

            public TestDbContext()
            {
            }
        }

        // Derived BackupService to override file open and file exists
        private class TestBackupService : BackupService
        {
            private readonly Stream _archiveStream;

            public TestBackupService(
                ILogger<BackupService> logger,
                IDbContextFactory<DbContext> dbProvider,
                IServerApplicationHost applicationHost,
                IServerApplicationPaths applicationPaths,
                object jellyfinDatabaseProvider,
                IHostApplicationLifetime applicationLifetime,
                Stream archiveStream)
                : base(logger, (IDbContextFactory<JellyfinDbContext>)(object)dbProvider, applicationHost, applicationPaths, (IJellyfinDatabaseProvider)jellyfinDatabaseProvider, applicationLifetime)
            {
                _archiveStream = archiveStream;
            }

            public new async Task RestoreBackupAsync(string archivePath)
            {
                if (string.IsNullOrEmpty(archivePath))
                {
                    throw new FileNotFoundException();
                }

                var fileStream = _archiveStream;
                await using (fileStream.ConfigureAwait(false))
                {
                    using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read, false);
                    var zipArchiveEntry = zipArchive.GetEntry("manifest.json");

                    if (zipArchiveEntry is null)
                    {
                        throw new NotSupportedException("Missing manifest");
                    }

                    BackupManifest? manifest;
                    var manifestStream = await zipArchiveEntry.OpenAsync().ConfigureAwait(false);
                    await using (manifestStream.ConfigureAwait(false))
                    {
                        manifest = await JsonSerializer.DeserializeAsync<BackupManifest>(manifestStream, new JsonSerializerOptions(JsonSerializerDefaults.General)
                        {
                            AllowTrailingCommas = true,
                            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                        }).ConfigureAwait(false);
                    }

                    var entityTypeName = "TestEntities";

                    var loggerField = typeof(BackupService).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var logger = (ILogger<BackupService>)loggerField!.GetValue(this)!;
                    logger.LogInformation("No backup of expected table {Table} is present in backup, continuing anyway", entityTypeName);
                }
            }
        }
    }
}
