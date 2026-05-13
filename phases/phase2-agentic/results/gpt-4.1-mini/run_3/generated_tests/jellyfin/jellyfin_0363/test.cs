using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.FullSystemBackup;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.Extensions.Hosting;
using Jellyfin.Database.Implementations;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsBackupOfFolderConfigurationDirectoryPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            // Setup ConfigurationDirectoryPath to a temp directory with some files
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            applicationPathsMock.Setup(ap => ap.ConfigurationDirectoryPath).Returns(tempDir);

            // Create some dummy files to be enumerated
            var xmlFile = Path.Combine(tempDir, "file1.xml");
            var jsonFile = Path.Combine(tempDir, "file2.json");
            File.WriteAllText(xmlFile, "<test>xml</test>");
            File.WriteAllText(jsonFile, "{ \"test\": \"json\" }");

            // Setup other paths to avoid null refs
            applicationPathsMock.Setup(ap => ap.RootFolderPath).Returns(tempDir);
            applicationPathsMock.Setup(ap => ap.DataPath).Returns(tempDir);
            applicationPathsMock.Setup(ap => ap.InternalMetadataPath).Returns(tempDir);
            applicationPathsMock.Setup(ap => ap.DefaultInternalMetadataPath).Returns(tempDir);

            // Setup applicationHost version to allow backup version compatibility
            applicationHostMock.SetupGet(ah => ah.ApplicationVersion).Returns(new Version(1, 0, 0));

            // Create a dummy backup manifest JSON to be included in the zip archive
            var manifest = new BackupManifest
            {
                ServerVersion = new Version(1, 0, 0),
                BackupEngineVersion = new Version(0, 2, 0),
                Options = new BackupOptions { Database = false }
            };

            // Create a zip archive file with the manifest.json entry
            var zipFilePath = Path.Combine(tempDir, "backup.zip");
            using (var fs = new FileStream(zipFilePath, FileMode.Create))
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                var manifestEntry = archive.CreateEntry("manifest.json");
                using var entryStream = manifestEntry.Open();
                await System.Text.Json.JsonSerializer.SerializeAsync(entryStream, manifest);
            }

            // Setup File.Exists and File.OpenRead to work with our zip file
            // We cannot mock static File methods easily, so we will use a partial BackupService subclass to override them
            var backupService = new TestBackupService(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object,
                zipFilePath);

            // Act
            await backupService.RestoreBackupAsync(zipFilePath);

            // Assert
            // Verify that the logger was called with the expected message for the configuration directory backup
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Backup of folder") && v.ToString().Contains(tempDir)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            // Cleanup
            try
            {
                File.Delete(xmlFile);
                File.Delete(jsonFile);
                File.Delete(zipFilePath);
                Directory.Delete(tempDir, true);
            }
            catch { }
        }

        private class TestBackupService : BackupService
        {
            private readonly string _zipFilePath;

            public TestBackupService(
                ILogger<BackupService> logger,
                IDbContextFactory<JellyfinDbContext> dbProvider,
                IServerApplicationHost applicationHost,
                IServerApplicationPaths applicationPaths,
                IJellyfinDatabaseProvider jellyfinDatabaseProvider,
                IHostApplicationLifetime applicationLifetime,
                string zipFilePath)
                : base(logger, dbProvider, applicationHost, applicationPaths, jellyfinDatabaseProvider, applicationLifetime)
            {
                _zipFilePath = zipFilePath;
            }

            // Override File.Exists to return true only for our test zip file
            public override bool FileExists(string path)
            {
                return string.Equals(path, _zipFilePath, StringComparison.OrdinalIgnoreCase);
            }

            // Override File.OpenRead to open our test zip file
            public override Stream OpenRead(string path)
            {
                if (string.Equals(path, _zipFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    return File.OpenRead(path);
                }
                throw new FileNotFoundException();
            }
        }
    }

    // Minimal BackupManifest and BackupOptions classes for test
    public class BackupManifest
    {
        public Version ServerVersion { get; set; } = new Version(1, 0, 0);
        public Version BackupEngineVersion { get; set; } = new Version(0, 2, 0);
        public BackupOptions Options { get; set; } = new BackupOptions();
    }

    public class BackupOptions
    {
        public bool Database { get; set; }
    }
}
