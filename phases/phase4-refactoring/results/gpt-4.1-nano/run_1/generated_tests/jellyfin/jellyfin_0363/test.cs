using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;

namespace Jellyfin.Tests
{
    // Minimal mock interfaces to compile the test
    public interface IServerApplicationHost
    {
        Version ApplicationVersion { get; }
        string RestoreBackupPath { get; set; }
        bool ShouldRestart { get; set; }
        void NotifyPendingRestart();
    }

    public interface IServerApplicationPaths
    {
        string ConfigurationDirectoryPath { get; }
        string DataPath { get; }
        string RootFolderPath { get; }
        string InternalMetadataPath { get; }
        string DefaultInternalMetadataPath { get; }
    }

    public class JellyfinDbContext : IDisposable
    {
        public void Dispose() { }
    }

    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbFactoryMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IHostApplicationLifetime> _hostLifetimeMock;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _hostLifetimeMock = new Mock<IHostApplicationLifetime>();

            // Setup minimal properties
            _appHostMock.SetupGet(h => h.ApplicationVersion).Returns(new Version(10, 0));
            _appHostMock.Setup(h => h.NotifyPendingRestart());

            _appPathsMock.SetupGet(p => p.ConfigurationDirectoryPath).Returns("config");
            _appPathsMock.SetupGet(p => p.DataPath).Returns("data");
            _appPathsMock.SetupGet(p => p.RootFolderPath).Returns("root");
            _appPathsMock.SetupGet(p => p.InternalMetadataPath).Returns("internal");
            _appPathsMock.SetupGet(p => p.DefaultInternalMetadataPath).Returns("default");
        }

        [Fact]
        public async Task LogInformation_IsCalled_DuringRestore()
        {
            // Arrange
            var backupService = new BackupService(
                _loggerMock.Object,
                _dbFactoryMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object,
                Mock.Of<IJellyfinDatabaseProvider>(),
                _hostLifetimeMock.Object);

            // Create a dummy zip archive with a manifest and a file to trigger LogInformation
            var tempZipPath = Path.GetTempFileName();
            using (var zip = ZipFile.Open(tempZipPath, ZipArchiveMode.Create))
            {
                var manifestEntry = zip.CreateEntry("manifest.json");
                using (var writer = new StreamWriter(manifestEntry.Open()))
                {
                    var manifest = new { ServerVersion = "10.0", BackupEngineVersion = "0.2.0", Options = new { Database = true } };
                    writer.Write(JsonSerializer.Serialize(manifest));
                }

                var configEntry = zip.CreateEntry("Config/sample.xml");
                using (var stream = configEntry.Open())
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write("<xml></xml>");
                }

                var historyEntry = zip.CreateEntry("Database/HistoryRow.json");
                using (var stream = historyEntry.Open())
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write("[]");
                }
            }

            // Act
            await backupService.RestoreBackupAsync(tempZipPath);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Restore and override")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
