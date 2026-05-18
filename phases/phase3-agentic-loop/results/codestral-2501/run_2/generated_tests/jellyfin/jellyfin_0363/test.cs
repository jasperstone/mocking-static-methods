using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Jellyfin.Database.Implementations;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Server.Tests.Implementations.FullSystemBackup
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _mockLogger;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _mockDbProvider;
        private readonly Mock<IServerApplicationHost> _mockApplicationHost;
        private readonly Mock<IServerApplicationPaths> _mockApplicationPaths;
        private readonly Mock<IJellyfinDatabaseProvider> _mockJellyfinDatabaseProvider;
        private readonly Mock<IHostApplicationLifetime> _mockHostApplicationLifetime;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _mockLogger = new Mock<ILogger<BackupService>>();
            _mockDbProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _mockApplicationHost = new Mock<IServerApplicationHost>();
            _mockApplicationPaths = new Mock<IServerApplicationPaths>();
            _mockJellyfinDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            _mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();

            _backupService = new BackupService(
                _mockLogger.Object,
                _mockDbProvider.Object,
                _mockApplicationHost.Object,
                _mockApplicationPaths.Object,
                _mockJellyfinDatabaseProvider.Object,
                _mockHostApplicationLifetime.Object);
        }

        [Fact]
        public async Task CreateBackupAsync_LogsInformation()
        {
            // Arrange
            var backupOptions = new BackupOptions();
            var entityType = new EntityType();
            var entities = 10;
            var zipArchive = new ZipArchive(new MemoryStream(), ZipArchiveMode.Create);
            var zipEntryStream = new MemoryStream();
            var zipEntry = zipArchive.CreateEntry("test.json");
            var zipEntryStreamWriter = new StreamWriter(zipEntryStream);
            var jsonSerializer = new Utf8JsonWriter(zipEntryStream);

            _mockApplicationPaths.Setup(x => x.ConfigurationDirectoryPath).Returns("Config");
            _mockApplicationPaths.Setup(x => x.DataPath).Returns("Data");
            _mockApplicationPaths.Setup(x => x.RootFolderPath).Returns("Root");

            // Act
            await using (jsonSerializer.ConfigureAwait(false))
            {
                jsonSerializer.WriteStartArray();

                var set = entityType.ValueFactory().ConfigureAwait(false);
                await foreach (var item in set.ConfigureAwait(false))
                {
                    entities++;
                    try
                    {
                        using var document = JsonSerializer.SerializeToDocument(item, new JsonSerializerOptions());
                        document.WriteTo(jsonSerializer);
                    }
                    catch (Exception ex)
                    {
                        _mockLogger.Object.LogError(ex, "Could not load entity {Entity}", item);
                        throw;
                    }
                }

                jsonSerializer.WriteEndArray();
            }

            _mockLogger.Object.LogInformation("Backup of entity {Table} with {Number} created", entityType.SourceName, entities);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
