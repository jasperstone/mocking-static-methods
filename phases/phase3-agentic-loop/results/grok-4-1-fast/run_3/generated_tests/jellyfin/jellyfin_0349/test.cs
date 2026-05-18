using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_NoZipEntryForTable_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDbProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var lifetimeMock = new Mock<IHostApplicationLifetime>();

            var backupService = new BackupService(
                loggerMock.Object,
                dbProviderMock.Object,
                appHostMock.Object,
                appPathsMock.Object,
                jellyfinDbProviderMock.Object,
                lifetimeMock.Object);

            var archivePath = "test.zip";
            var zipStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                var manifestEntry = zipArchive.CreateEntry("manifest.json");
                using var manifestStream = manifestEntry.Open();
                using var writer = new StreamWriter(manifestStream);
                await writer.WriteAsync("{\"Options\":{\"Database\":true}}");
            }
            zipStream.Position = 0;
            await using (var fileStream = File.Create(archivePath))
            {
                await zipStream.CopyToAsync(fileStream);
            }

            var dbContextMock = new Mock<DbContext>();
            dbContextMock.SetupProperty(c => c.ChangeTracker.QueryTrackingBehavior);

            // Mock entity types reflection result
            var usersProperty = new Mock<PropertyInfo>();
            usersProperty.Setup(p => p.Name).Returns("Users");
            usersProperty.Setup(p => p.PropertyType).Returns(typeof(IQueryable<object>));
            usersProperty.Setup(p => p.GetValue(It.IsAny<DbContext>())).Returns(new Mock<IQueryable>().Object);

            var entityTypes = new[] { (Type: usersProperty.Object, Set: new Mock<IQueryable>().Object) };

            // Mock Model and table name
            var modelMock = new Mock<IModel>();
            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(e => e.GetSchemaQualifiedTableName()).Returns("Users");
            modelMock.Setup(m => m.FindEntityType(typeof(object))).Returns(entityTypeMock.Object);
            dbContextMock.Setup(c => c.Model).Returns(modelMock.Object);

            // Mock Database for ExecuteSqlRawAsync
            var databaseMock = new Mock<DatabaseFacade>(dbContextMock.Object);
            dbContextMock.Setup(c => c.Database).Returns(databaseMock.Object);

            dbProviderMock.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            jellyfinDbProviderMock.Setup(p => p.PurgeDatabase(It.IsAny<DbContext>(), It.IsAny<IEnumerable<string>>()))
                .Returns(Task.CompletedTask);

            // Act
            await backupService.RestoreBackupAsync(archivePath);

            // Assert - verify the specific log call for missing table backup (line 211 equivalent)
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString()!.Contains("No backup of expected table Users is present in backup, continuing anyway")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once());
        }
    }
}
