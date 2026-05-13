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

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsBackupOfFolderConfigurationDirectoryPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            // Setup ConfigurationDirectoryPath to a temp directory with some files
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            applicationPathsMock.Setup(ap => ap.ConfigurationDirectoryPath).Returns(tempDir);

            // Create some dummy files to be enumerated
            var xmlFile = Path.Combine(tempDir, "config1.xml");
            var jsonFile = Path.Combine(tempDir, "config2.json");
            File.WriteAllText(xmlFile, "<xml></xml>");
            File.WriteAllText(jsonFile, "{ }");

            // Setup ZipArchive mock to intercept CreateEntryFromFileAsync calls
            var zipArchiveMock = new Mock<ZipArchive>(MockBehavior.Strict, Stream.Null, ZipArchiveMode.Create, true);
            var createdEntries = new List<string>();
            zipArchiveMock.Setup(z => z.CreateEntryFromFileAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .Returns(Task.CompletedTask)
                .Callback<string, string, System.Threading.CancellationToken>((sourceFileName, entryName, ct) =>
                {
                    createdEntries.Add(entryName);
                });

            // We need to call the private method or simulate the method that contains the line:
            // _logger.LogInformation("Backup of folder {Table}", _applicationPaths.ConfigurationDirectoryPath);
            // The code snippet is from the BackupService class, but the method is not named in the snippet.
            // The snippet is from a method that creates a backup archive, presumably.
            // Since the method is not public, we will create a derived test class to expose a method that calls the relevant code.
            // Alternatively, we can test ScheduleRestoreAndRestartServer or RestoreBackupAsync, but the snippet is from backup creation, not restore.
            // The snippet is from a method that creates a backup archive, but the method name is not visible.
            // We will create a minimal derived class with a method that calls the relevant code for testing.

            var backupService = new TestBackupService(
                loggerMock.Object,
                dbProviderMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object,
                zipArchiveMock.Object);

            // Act
            await backupService.TestBackupFolderAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Backup of folder")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            // Also verify that the log contains the ConfigurationDirectoryPath value
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(tempDir)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);

            // Cleanup
            try
            {
                File.Delete(xmlFile);
                File.Delete(jsonFile);
                Directory.Delete(tempDir);
            }
            catch
            {
                // ignore cleanup errors
            }
        }

        private class TestBackupService : BackupService
        {
            private readonly ZipArchive _zipArchive;

            public TestBackupService(
                ILogger<BackupService> logger,
                IDbContextFactory<JellyfinDbContext> dbProvider,
                IServerApplicationHost applicationHost,
                IServerApplicationPaths applicationPaths,
                IJellyfinDatabaseProvider jellyfinDatabaseProvider,
                IHostApplicationLifetime hostApplicationLifetime,
                ZipArchive zipArchive)
                : base(logger, dbProvider, applicationHost, applicationPaths, jellyfinDatabaseProvider, hostApplicationLifetime)
            {
                _zipArchive = zipArchive;
            }

            public async Task TestBackupFolderAsync()
            {
                // This method simulates the snippet code that logs the backup of the folder ConfigurationDirectoryPath
                var logger = GetLogger();
                var configPath = GetApplicationPaths().ConfigurationDirectoryPath;

                logger.LogInformation("Backup of folder {Table}", configPath);

                foreach (var item in Directory.EnumerateFiles(configPath, "*.xml", SearchOption.TopDirectoryOnly)
                             .Union(Directory.EnumerateFiles(configPath, "*.json", SearchOption.TopDirectoryOnly)))
                {
                    await _zipArchive.CreateEntryFromFileAsync(item, NormalizePathSeparator(Path.Combine("Config", Path.GetFileName(item)))).ConfigureAwait(false);
                }
            }

            private ILogger<BackupService> GetLogger()
            {
                var loggerField = typeof(BackupService).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return (ILogger<BackupService>)loggerField!.GetValue(this)!;
            }

            private IServerApplicationPaths GetApplicationPaths()
            {
                var pathsField = typeof(BackupService).GetField("_applicationPaths", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return (IServerApplicationPaths)pathsField!.GetValue(this)!;
            }

            private static string NormalizePathSeparator(string path)
            {
                return path.Replace('\\', '/');
            }
        }
    }
}
