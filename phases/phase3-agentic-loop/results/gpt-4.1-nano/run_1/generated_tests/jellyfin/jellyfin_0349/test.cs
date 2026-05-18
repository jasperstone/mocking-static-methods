using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbFactoryMock;
        private readonly Mock<IHostApplicationLifetime> _hostLifetimeMock;
        private readonly Mock<IJellyfinDatabaseProvider> _databaseProviderMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _hostLifetimeMock = new Mock<IHostApplicationLifetime>();
            _databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
        }

        [Fact]
        public async Task LogInformation_Is_Called_During_RestoreBackupAsync()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var archivePath = Path.Combine(tempDir, "backup.zip");
            var zipStream = new MemoryStream();

            // Create a dummy zip archive with manifest and a dummy database entry
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                var manifestEntry = archive.CreateEntry("manifest.json");
                using (var writer = new StreamWriter(manifestEntry.Open()))
                {
                    var manifest = new BackupManifest
                    {
                        ServerVersion = new Version(1, 0, 0),
                        BackupEngineVersion = new Version(0, 2, 0)
                    };
                    JsonSerializer.Serialize(writer, manifest);
                }

                var dbEntry = archive.CreateEntry("Database/HistoryRow.json");
                using (var entryStream = dbEntry.Open())
                using (var writer = new StreamWriter(entryStream))
                {
                    writer.Write("[]");
                }
            }
            zipStream.Seek(0, SeekOrigin.Begin);
            await File.WriteAllBytesAsync(archivePath, zipStream.ToArray());

            var backupService = new BackupService(
                _loggerMock.Object,
                _dbFactoryMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object,
                _databaseProviderMock.Object,
                _hostLifetimeMock.Object);

            // Mock dependencies
            var dbContextMock = new Mock<JellyfinDbContext>();
            _dbFactoryMock.Setup(f => f.CreateDbContextAsync()).ReturnsAsync(dbContextMock.Object);
            var databaseMock = new Mock<IDatabaseFacade>();
            dbContextMock.Setup(c => c.Database).Returns(databaseMock.Object);
            databaseMock.Setup(d => d.ExecuteSqlRawAsync(It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(1);
            // Act
            await backupService.RestoreBackupAsync(archivePath);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Begin restoring Database")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
