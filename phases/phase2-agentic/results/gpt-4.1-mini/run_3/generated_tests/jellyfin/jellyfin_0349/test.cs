using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbContextFactoryMock;
        private readonly Mock<IServerApplicationHost> _applicationHostMock;
        private readonly Mock<IServerApplicationPaths> _applicationPathsMock;
        private readonly Mock<IJellyfinDatabaseProvider> _databaseProviderMock;
        private readonly Mock<IHostApplicationLifetime> _hostApplicationLifetimeMock;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _applicationHostMock = new Mock<IServerApplicationHost>();
            _applicationPathsMock = new Mock<IServerApplicationPaths>();
            _databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            _hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsInformationWhenNoBackupOfExpectedTable()
        {
            // Arrange
            var backupService = new BackupService(
                _loggerMock.Object,
                _dbContextFactoryMock.Object,
                _applicationHostMock.Object,
                _applicationPathsMock.Object,
                _databaseProviderMock.Object,
                _hostApplicationLifetimeMock.Object);

            // Setup a temporary zip archive in memory with minimal structure
            using var memStream = new MemoryStream();
            using (var archive = new ZipArchive(memStream, ZipArchiveMode.Create, true))
            {
                // Add manifest.json entry
                var manifestEntry = archive.CreateEntry("manifest.json");
                using (var writer = new StreamWriter(manifestEntry.Open()))
                {
                    // Write a minimal valid manifest json with compatible versions
                    writer.Write("{\"ServerVersion\":\"1.0.0\",\"BackupEngineVersion\":\"0.2.0\",\"Options\":{\"Database\":true}}");
                }

                // Add HistoryRow.json entry to simulate migration history presence
                var historyEntry = archive.CreateEntry("Database/HistoryRow.json");
                using (var writer = new StreamWriter(historyEntry.Open()))
                {
                    writer.Write("[]");
                }

                // Add a table json entry for one entity type
                var tableEntry = archive.CreateEntry("Database/FakeEntity.json");
                using (var writer = new StreamWriter(tableEntry.Open()))
                {
                    writer.Write("[]");
                }
            }
            memStream.Seek(0, SeekOrigin.Begin);

            // Mock File.Exists to true for the archive path
            var archivePath = "fakepath.zip";
            // We cannot mock static File.Exists easily, so we will override RestoreBackupAsync to accept Stream for test or simulate by writing to disk
            // Instead, we will create a derived class for testing that overrides File.Exists and File.OpenRead

            var testBackupService = new TestBackupService(
                _loggerMock.Object,
                _dbContextFactoryMock.Object,
                _applicationHostMock.Object,
                _applicationPathsMock.Object,
                _databaseProviderMock.Object,
                _hostApplicationLifetimeMock.Object,
                memStream);

            // Setup DbContext and related mocks
            var dbContextMock = new Mock<JellyfinDbContext>();
            var databaseMock = new Mock<DatabaseFacade>(dbContextMock.Object);
            dbContextMock.SetupGet(d => d.Database).Returns(databaseMock.Object);
            dbContextMock.SetupGet(d => d.ChangeTracker).Returns(Mock.Of<ChangeTracker>());
            dbContextMock.SetupGet(d => d.Model).Returns(Mock.Of<IModel>());

            _dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContextMock.Object);

            // Setup entity types reflection to simulate one entity type property
            var entityTypeProperty = typeof(FakeDbContext).GetProperty(nameof(FakeDbContext.FakeEntities));
            var entityTypes = new[]
            {
                (Type: entityTypeProperty, Set: (IQueryable)Enumerable.Empty<FakeEntity>().AsQueryable())
            };

            // Setup reflection on JellyfinDbContext to return our fake entity types
            // We will mock the GetProperties call by creating a derived DbContext with the property
            // But since BackupService uses typeof(JellyfinDbContext).GetProperties, we cannot mock that easily
            // Instead, we will mock the Model.FindEntityType to return a fake IEntityType with GetSchemaQualifiedTableName

            var modelMock = new Mock<IModel>();
            var entityTypeMock = new Mock<Microsoft.EntityFrameworkCore.Metadata.IEntityType>();
            entityTypeMock.Setup(e => e.GetSchemaQualifiedTableName()).Returns("FakeSchema.FakeTable");
            modelMock.Setup(m => m.FindEntityType(It.IsAny<Type>())).Returns(entityTypeMock.Object);
            dbContextMock.SetupGet(d => d.Model).Returns(modelMock.Object);

            // Setup _jellyfinDatabaseProvider.PurgeDatabase to complete successfully
            _databaseProviderMock.Setup(p => p.PurgeDatabase(dbContextMock.Object, It.IsAny<IEnumerable<string>>())).Returns(Task.CompletedTask);

            // Setup zipArchive.GetEntry to return null for the table to trigger the log on line 211
            // We will override the zipArchive in the test class to simulate this behavior

            // Act
            await testBackupService.RestoreBackupAsync("fakepath.zip");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No backup of expected table")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        // Helper classes for test

        private class TestBackupService : BackupService
        {
            private readonly Stream _zipStream;

            public TestBackupService(
                ILogger<BackupService> logger,
                IDbContextFactory<JellyfinDbContext> dbProvider,
                IServerApplicationHost applicationHost,
                IServerApplicationPaths applicationPaths,
                IJellyfinDatabaseProvider jellyfinDatabaseProvider,
                IHostApplicationLifetime applicationLifetime,
                Stream zipStream)
                : base(logger, dbProvider, applicationHost, applicationPaths, jellyfinDatabaseProvider, applicationLifetime)
            {
                _zipStream = zipStream;
            }

            protected override bool FileExists(string path)
            {
                return true;
            }

            protected override Stream OpenRead(string path)
            {
                _zipStream.Seek(0, SeekOrigin.Begin);
                return _zipStream;
            }
        }

        // Dummy DbContext and entity for reflection simulation
        public class FakeDbContext : DbContext
        {
            public IQueryable<FakeEntity> FakeEntities => new List<FakeEntity>().AsQueryable();
        }

        public class FakeEntity
        {
            public int Id { get; set; }
        }
    }
}
