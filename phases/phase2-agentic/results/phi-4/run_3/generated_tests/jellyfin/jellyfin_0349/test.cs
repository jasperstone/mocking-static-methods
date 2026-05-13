using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class BackupServiceTests
{
    [Fact]
    public async Task LogInformationCalled_WhenNoBackupOfExpectedTable()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<BackupService>>();
        var dbContextMock = new Mock<JellyfinDbContext>();
        var historyRepositoryMock = new Mock<IHistoryRepository>();
        var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
        var applicationPathsMock = new Mock<IServerApplicationPaths>();
        var applicationHostMock = new Mock<IServerApplicationHost>();
        var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

        var backupService = new BackupService(
            loggerMock.Object,
            new TestDbContextFactory(dbContextMock.Object),
            applicationHostMock.Object,
            applicationPathsMock.Object,
            jellyfinDatabaseProviderMock.Object,
            hostApplicationLifetimeMock.Object);

        var zipArchiveMock = new Mock<ZipArchive>();
        var zipEntryMock = new Mock<ZipArchiveEntry>();
        zipEntryMock.Setup(e => e.FullName).Returns("Database/NonExistentTable.json");
        zipArchiveMock.Setup(a => a.GetEntry(It.IsAny<string>())).Returns((ZipArchiveEntry)null);

        // Act
        await backupService.RestoreBackupAsync("dummyPath");

        // Assert
        loggerMock.Verify(
            l => l.LogInformation(
                It.Is<string>(s => s.Contains("No backup of expected table")),
                It.Is<object[]>(o => o.Length == 1 && o[0].ToString() == "NonExistentTable")),
            Times.Once);
    }
}

public class TestDbContextFactory : IDbContextFactory<JellyfinDbContext>
{
    private readonly JellyfinDbContext _dbContext;

    public TestDbContextFactory(JellyfinDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public JellyfinDbContext CreateDbContext()
    {
        return _dbContext;
    }

    public Task<JellyfinDbContext> CreateDbContextAsync()
    {
        return Task.FromResult(_dbContext);
    }
}
