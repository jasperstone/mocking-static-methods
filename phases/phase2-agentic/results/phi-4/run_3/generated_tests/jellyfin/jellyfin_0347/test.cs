using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Jellyfin.Server.Implementations.StorageHelpers;
using Jellyfin.Server.Implementations.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class BackupServiceTests
{
    [Fact]
    public async Task DatabasePurged_LogInformationCalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BackupService>>();
        var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var applicationHostMock = new Mock<IServerApplicationHost>();
        var applicationPathsMock = new Mock<IServerApplicationPaths>();
        var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
        var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

        var dbContextMock = new Mock<JellyfinDbContext>();
        dbProviderMock.Setup(dp => dp.CreateDbContextAsync()).ReturnsAsync(dbContextMock.Object);

        var backupService = new BackupService(
            loggerMock.Object,
            dbProviderMock.Object,
            applicationHostMock.Object,
            applicationPathsMock.Object,
            jellyfinDatabaseProviderMock.Object,
            hostApplicationLifetimeMock.Object);

        // Create a mock ZipArchive and ZipArchiveEntry for the test
        var zipArchiveMock = new Mock<ZipArchive>();
        var zipEntryMock = new Mock<ZipArchiveEntry>();
        zipArchiveMock.Setup(z => z.GetEntry(It.IsAny<string>())).Returns(zipEntryMock.Object);

        // Mock the OpenAsync method to return a stream
        var stream = new MemoryStream();
        zipEntryMock.Setup(e => e.OpenAsync()).ReturnsAsync(stream);

        // Mock the DeserializeAsyncEnumerable method to return an empty enumerable
        var jsonSerializerSettings = new JsonSerializerOptions();
        var jsonSerializerMock = new Mock<JsonSerializer>();
        jsonSerializerMock.Setup(j => j.DeserializeAsyncEnumerable<JsonObject>(It.IsAny<Stream>(), It.IsAny<JsonSerializerOptions>()))
            .ReturnsAsync(Enumerable.Empty<JsonObject>());

        // Act
        await backupService.RestoreBackupAsync("dummyPath");

        // Assert
        loggerMock.Verify(
            l => l.LogInformation("Database Purged"),
            Times.Once);
    }
}
