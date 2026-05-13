using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Tests
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbFactoryMock;
        private readonly Mock<IJellyfinDatabaseProvider> _databaseProviderMock;
        private readonly Mock<IServerApplicationHost> _applicationHostMock;
        private readonly Mock<IHostApplicationLifetime> _hostLifetimeMock;
        private readonly Mock<ILogger> _loggerGenericMock;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            _applicationHostMock = new Mock<IServerApplicationHost>();
            _hostLifetimeMock = new Mock<IHostApplicationLifetime>();
            _loggerGenericMock = new Mock<ILogger>();
        }

        [Fact]
        public async Task RestoreBackupAsync_ShouldLogInformation_WhenEntryIsNull()
        {
            // Arrange
            var backupService = new BackupService(
                _loggerMock.Object,
                _dbFactoryMock.Object,
                _applicationHostMock.Object,
                Mock.Of<IServerApplicationPaths>(),
                _databaseProviderMock.Object,
                _hostLifetimeMock.Object);

            var tempFile = Path.GetTempFileName();
            try
            {
                using (var zip = new ZipArchive(File.Create(tempFile), ZipArchiveMode.Create))
                {
                    var manifestEntry = zip.CreateEntry("manifest.json");
                    await using (var stream = manifestEntry.Open())
                    {
                        await JsonSerializer.SerializeAsync(stream, new BackupManifest { ServerVersion = new Version(1, 0, 0), BackupEngineVersion = new Version(0, 2, 0) });
                    }
                }

                // Act
                await backupService.RestoreBackupAsync(tempFile);

                // Assert
                _loggerMock.Verify(x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Begin restoring system to")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
