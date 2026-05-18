using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Jellyfin.Server.Implementations.StorageHelpers;
using MediaBrowser.Controller;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Jellyfin.Server.Tests.Implementations.FullSystemBackup
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;
        private readonly Mock<IServerApplicationHost> _applicationHostMock;
        private readonly Mock<IServerApplicationPaths> _applicationPathsMock;
        private readonly Mock<IJellyfinDatabaseProvider> _jellyfinDatabaseProviderMock;
        private readonly Mock<IHostApplicationLifetime> _hostApplicationLifetimeMock;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _applicationHostMock = new Mock<IServerApplicationHost>();
            _applicationPathsMock = new Mock<IServerApplicationPaths>();
            _jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            _hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            _backupService = new BackupService(
                _loggerMock.Object,
                _dbProviderMock.Object,
                _applicationHostMock.Object,
                _applicationPathsMock.Object,
                _jellyfinDatabaseProviderMock.Object,
                _hostApplicationLifetimeMock.Object);
        }

        [Fact]
        public async Task BackupService_LogInformation_CalledWithCorrectParameters()
        {
            // Arrange
            var backupOptions = new BackupOptions
            {
                Metadata = true,
                Trickplay = true,
                Subtitles = true,
                Database = true
            };

            var entityType = new EntityType
            {
                SourceName = "TestEntity",
                ValueFactory = () => Task.FromResult(new List<object> { new { Id = 1, Name = "Test" } }.ToAsyncEnumerable())
            };

            var zipArchiveMock = new Mock<ZipArchive>();
            var zipEntryStreamMock = new Mock<Stream>();
            var zipEntryMock = new Mock<ZipArchiveEntry>();

            zipArchiveMock.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(zipEntryMock.Object);
            zipEntryMock.Setup(x => x.Open()).Returns(zipEntryStreamMock.Object);

            _applicationPathsMock.Setup(x => x.ConfigurationDirectoryPath).Returns("TestPath");
            _applicationPathsMock.Setup(x => x.RootFolderPath).Returns("TestPath");
            _applicationPathsMock.Setup(x => x.DataPath).Returns("TestPath");

            // Act
            await _backupService.CreateBackupAsync(zipArchiveMock.Object, backupOptions, new List<EntityType> { entityType });

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Exactly(7));
        }
    }
}
