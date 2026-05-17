using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Jellyfin.Server.Implementations.SystemBackupService;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class BackupServiceTests
{
    [Fact]
    public async Task LogInformation_ShouldBeCalled_WithExpectedParameters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<BackupService>>();
        var mockDbProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var mockApplicationHost = new Mock<IServerApplicationHost>();
        var mockApplicationPaths = new Mock<IServerApplicationPaths>();
        var mockJellyfinDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
        var mockHostApplicationLifetime = new Mock<IHostApplicationLifetime>();

        var backupService = new BackupService(
            mockLogger.Object,
            mockDbProvider.Object,
            mockApplicationHost.Object,
            mockApplicationPaths.Object,
            mockJellyfinDatabaseProvider.Object,
            mockHostApplicationLifetime.Object);

        var mockEntityType = new Mock<IEntityType>();
        mockEntityType.Setup(e => e.SourceName).Returns("TestTable");
        mockEntityType.Setup(e => e.ValueFactory()).Returns(Task.FromResult(new Mock<IAsyncEnumerable<object>>().Object));

        var mockZipArchive = new Mock<ZipArchive>();
        var mockZipEntryStream = new MemoryStream();
        var mockJsonSerializer = new Mock<Utf8JsonWriter>(mockZipEntryStream);

        // Act
        await backupService.BackupEntitiesAsync(mockEntityType.Object, mockZipArchive.Object);

        // Assert
        mockLogger.Verify(
            logger => logger.LogInformation(
                It.Is<string>(s => s == "Backup of entity {Table} with {Number} created"),
                It.Is<object>(o => o == "TestTable"),
                It.Is<int>(i => i > 0)), // Assuming entities > 0
            Times.Once);
    }
}
