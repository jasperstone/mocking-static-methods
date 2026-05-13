using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.StorageHelpers;
using Jellyfin.Server.Implementations.SystemBackupService;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
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
            var entityType = new EntityType
            {
                SourceName = "TestEntity",
                ValueFactory = () => Task.FromResult(new List<object> { new object() }.ToAsyncEnumerable())
            };

            var zipArchiveMock = new Mock<ZipArchive>();
            var zipEntryMock = new Mock<ZipArchiveEntry>();
            zipArchiveMock.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(zipEntryMock.Object);
            zipEntryMock.Setup(x => x.Open()).Returns(new MemoryStream());

            _applicationPathsMock.Setup(x => x.ConfigurationDirectoryPath).Returns("TestPath");

            // Act
            await _backupService.BackupAsync(zipArchiveMock.Object, new BackupOptions(), new List<EntityType> { entityType });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Backup of entity TestEntity with 1 created")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task BackupService_LogInformation_CalledWithCorrectParametersForConfigurationDirectory()
        {
            // Arrange
            var zipArchiveMock = new Mock<ZipArchive>();
            var zipEntryMock = new Mock<ZipArchiveEntry>();
            zipArchiveMock.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(zipEntryMock.Object);
            zipEntryMock.Setup(x => x.Open()).Returns(new MemoryStream());

            _applicationPathsMock.Setup(x => x.ConfigurationDirectoryPath).Returns("TestPath");

            // Act
            await _backupService.BackupAsync(zipArchiveMock.Object, new BackupOptions(), new List<EntityType>());

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Backup of folder TestPath")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }

    public class EntityType
    {
        public string SourceName { get; set; }
        public Func<Task<IAsyncEnumerable<object>>> ValueFactory { get; set; }
    }
}
