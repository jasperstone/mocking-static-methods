using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IDbContextFactory<DbContext>> _dbProviderMock;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbProviderMock = new Mock<IDbContextFactory<DbContext>>();

            // Minimal mocks for Jellyfin-specific interfaces using object
            var applicationHostMock = new Mock<object>();
            var applicationPathsMock = new Mock<object>();
            var jellyfinDatabaseProviderMock = new Mock<object>();
            var applicationLifetimeMock = new Mock<object>();

            _backupService = new BackupService(
                _loggerMock.Object,
                _dbProviderMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                applicationLifetimeMock.Object);
        }

        [Fact]
        public async Task RestoreBackupAsync_MissingZipEntryForTable_LogsInformationMessage()
        {
            // Arrange
            var archivePath = "test.zip";
            
            // Mock static File methods
            Mock.Get(File).Setup(f => f.Exists(archivePath)).Returns(true);
            
            var fileStream = new MemoryStream();
            Mock.Get(File).Setup(f => f.OpenRead(archivePath)).Returns(fileStream);
            
            // Create ZipArchive with manifest to pass initial checks
            using var tempZip = new ZipArchive(fileStream, ZipArchiveMode.Create, leaveOpen: true);
            var manifestEntry = tempZip.CreateEntry("manifest.json");
            using var manifestStream = manifestEntry.Open();
            var manifest = new 
            { 
                ServerVersion = "10.8.0",
                BackupEngineVersion = "0.2.0",
                Options = new { Database = true }
            };
            await JsonSerializer.SerializeAsync(manifestStream, manifest);
            
            fileStream.Position = 0;
            
            // Mock DbContext creation and database operations
            var dbContextMock = new Mock<DbContext>();
            _dbProviderMock.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(dbContextMock.Object);
            
            var databaseMock = new Mock<DatabaseFacade>(dbContextMock.Object);
            dbContextMock.Setup(c => c.Database).Returns(databaseMock.Object);
            databaseMock.Setup(d => d.ExecuteSqlRawAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);
            
            // Mock reflection properties to return controlled entity types
            var entityTypePropertyMock = new Mock<PropertyInfo>();
            entityTypePropertyMock.Setup(p => p.PropertyType).Returns(typeof(IQueryable<>).MakeGenericType(typeof(object)));
            entityTypePropertyMock.Setup(p => p.GetValue(dbContextMock.Object)).Returns(new Mock<IQueryable>().Object);
            
            dbContextMock.Setup(c => c.Model).Returns(new Mock<IModel>().Object);
            dbContextMock.Setup(c => c.ChangeTracker).Returns(new Mock<ChangeTracker>().Object);
            
            // Mock IJellyfinDatabaseProvider.PurgeDatabase
            var jellyfinDbProviderMock = (Mock<object>)_backupService.GetType()
                .GetField("_jellyfinDatabaseProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(_backupService);
            ((Mock<object>)jellyfinDbProviderMock).Setup(p => p.Setup(x => x.PurgeDatabase(It.IsAny<DbContext>(), It.IsAny<IEnumerable<string>>())))
                .Returns(Task.CompletedTask);
            
            // Mock ZipArchive.GetEntry to return null for table entries (triggers line 211)
            Mock.Get(ZipArchive).Setup(a => a.GetEntry(It.IsAny<string>()))
                .Returns((ZipArchiveEntry)null);

            // Act
            await _backupService.RestoreBackupAsync(archivePath);

            // Assert - verify the specific LogInformation call at line 211
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => 
                        t.ToString().Contains("No backup of expected table {Table}") &&
                        t.ToString().Contains("is present in backup, continuing anyway")),
                    It.Is<Exception>(e => e == null)),
                Times.AtLeastOnce);
        }
    }
}
