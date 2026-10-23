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
using Jellyfin.Database;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Jellyfin.Server.Implementations.SystemBackupService;
using MediaBrowser.Controller;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
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
        public async Task RestoreBackupAsync_LogsNoBackupOfExpectedTable_WhenZipEntryMissing()
        {
            // Arrange - Create minimal valid zip with manifest
            var memoryStream = CreateMinimalValidZip();

            // Mock File static method using Moq
            var fileMock = new Mock<FileStream>(memoryStream, FileAccess.Read);
            Mock.Get(File)
                .Setup(x => x.OpenRead(It.IsAny<string>()))
                .Returns(fileMock.Object);

            // Mock application paths to avoid storage check failures
            SetupApplicationPaths();
            _applicationHostMock.Setup(x => x.ApplicationVersion).Returns(new Version(10, 8, 0));

            // Mock dbContext with one IQueryable property to trigger the loop
            var dbContextMock = CreateDbContextMockWithQueryableProperty("Users");
            _dbProviderMock.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            // Mock jellyfinDatabaseProvider to avoid purge errors
            _jellyfinDatabaseProviderMock.Setup(x => x.PurgeDatabase(It.IsAny<JellyfinDbContext>(), It.IsAny<string[]>()))
                .Returns(Task.CompletedTask);

            // Act
            await _backupService.RestoreBackupAsync("test.zip");

            // Assert - Verify the specific LogInformation call at line 211
            _loggerMock.Verify(
                x => x.LogInformation(
                    "No backup of expected table {Table} is present in backup, continuing anyway",
                    "Users"),
                Times.Exactly(1));
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsReadBackupMessage_BeforeCheckingZipEntry()
        {
            // Arrange - Similar setup but verify the preceding log call
            var memoryStream = CreateMinimalValidZip();
            var fileMock = new Mock<FileStream>(memoryStream, FileAccess.Read);
            Mock.Get(File)
                .Setup(x => x.OpenRead(It.IsAny<string>()))
                .Returns(fileMock.Object);

            SetupApplicationPaths();
            _applicationHostMock.Setup(x => x.ApplicationVersion).Returns(new Version(10, 8, 0));

            var dbContextMock = CreateDbContextMockWithQueryableProperty("Users");
            _dbProviderMock.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            _jellyfinDatabaseProviderMock.Setup(x => x.PurgeDatabase(It.IsAny<JellyfinDbContext>(), It.IsAny<string[]>()))
                .Returns(Task.CompletedTask);

            // Act
            await _backupService.RestoreBackupAsync("test.zip");

            // Assert - Verify the "Read backup of {Table}" log that precedes line 211
            _loggerMock.Verify(
                x => x.LogInformation("Read backup of {Table}", "Users"),
                Times.Exactly(1));
        }

        private static MemoryStream CreateMinimalValidZip()
        {
            var memoryStream = new MemoryStream();
            using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                var manifestEntry = zip.CreateEntry("manifest.json");
                using var manifestStream = manifestEntry.Open();
                var manifest = new BackupManifest
                {
                    ServerVersion = new Version(10, 0, 0),
                    BackupEngineVersion = new Version(0, 2, 0),
                    Options = new BackupOptions { Database = true }
                };
                JsonSerializer.SerializeAsync(manifestStream, manifest).AsTask().Wait();
            }
            memoryStream.Position = 0;
            return memoryStream;
        }

        private void SetupApplicationPaths()
        {
            _applicationPathsMock.Setup(x => x.ConfigurationDirectoryPath).Returns("/config");
            _applicationPathsMock.Setup(x => x.DataPath).Returns("/data");
            _applicationPathsMock.Setup(x => x.RootFolderPath).Returns("/root");
            _applicationPathsMock.Setup(x => x.InternalMetadataPath).Returns("/metadata");
            _applicationPathsMock.Setup(x => x.DefaultInternalMetadataPath).Returns("/metadata-default");
        }

        private Mock<JellyfinDbContext> CreateDbContextMockWithQueryableProperty(string tableName)
        {
            var dbContextMock = new Mock<JellyfinDbContext>(new DbContextOptionsBuilder<JellyfinDbContext>().Options);

            // Mock Model.FindEntityType to return table name
            var modelMock = new Mock<IModel>();
            modelMock.Setup(x => x.FindEntityType(It.IsAny<Type>()))
                .Returns(new Mock<IEntityType>().Object);
            dbContextMock.Setup(x => x.Model).Returns(modelMock.Object);

            // Mock reflection to return one IQueryable property
            var propertyMock = new Mock<PropertyInfo>();
            propertyMock.Setup(p => p.PropertyType).Returns(typeof(IQueryable<>).MakeGenericType(typeof(object)));
            propertyMock.Setup(p => p.Name).Returns(tableName);
            propertyMock.Setup(p => p.GetValue(It.IsAny<object>())).Returns(new Mock<IQueryable>().Object);

            Mock.Get(typeof(JellyfinDbContext))
                .Setup(x => x.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                .Returns(new[] { propertyMock.Object });

            dbContextMock.Setup(x => x.ChangeTracker).Returns(new Mock<ChangeTracker>().Object);
            dbContextMock.Setup(x => x.Database).Returns(new Mock<DatabaseFacade>(dbContextMock.Object).Object);

            return dbContextMock;
        }
    }

    // Minimal types needed for compilation
    public class BackupManifest
    {
        public Version ServerVersion { get; set; } = null!;
        public Version BackupEngineVersion { get; set; } = null!;
        public BackupOptions Options { get; set; } = null!;
    }

    public class BackupOptions
    {
        public bool Database { get; set; }
    }
}
