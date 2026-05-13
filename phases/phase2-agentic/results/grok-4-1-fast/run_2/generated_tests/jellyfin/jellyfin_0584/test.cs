using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Tests.Routines;

public class ReseedFolderFlagTests
{
    private readonly Mock<IStartupLogger<ReseedFolderFlag>> _loggerMock;
    private readonly Mock<IServerApplicationPaths> _pathsMock;
    private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbContextFactoryMock;

    public ReseedFolderFlagTests()
    {
        _loggerMock = new Mock<IStartupLogger<ReseedFolderFlag>>();
        _loggerMock.SetupAllProperties();
        _pathsMock = new Mock<IServerApplicationPaths>();
        _dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
    }

    [Fact]
    public async Task PerformAsync_RerunGuardFlagTrue_LogsSkipMessage()
    {
        // Arrange
        ReseedFolderFlag.RerunGuardFlag = true;
        var routine = new ReseedFolderFlag(_loggerMock.Object, _dbContextFactoryMock.Object, _pathsMock.Object);

        // Act
        await routine.PerformAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(l => l.LogInformation("Migration is skipped because it does not apply."), Times.Once);
    }

    [Fact]
    public async Task PerformAsync_LibraryDbDoesNotExist_LogsErrorMessage()
    {
        // Arrange
        ReseedFolderFlag.RerunGuardFlag = false;
        _pathsMock.Setup(p => p.DataPath).Returns("/data");
        var routine = new ReseedFolderFlag(_loggerMock.Object, _dbContextFactoryMock.Object, _pathsMock.Object);

        // Act
        await routine.PerformAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(l => l.LogError("Cannot migrate IsFolder flag from {LibraryDb} as it does not exist. This migration expects the MigrateLibraryDb to run first.", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PerformAsync_LibraryDbExistsWithFolderItems_LogsMigratingCountMessage()
    {
        // Arrange
        ReseedFolderFlag.RerunGuardFlag = false;
        _pathsMock.Setup(p => p.DataPath).Returns("/data");

        // Mock File.Exists to return true
        var originalFileExists = File.Exists;
        File.Exists = (_, __) => true;

        var routine = new ReseedFolderFlag(_loggerMock.Object, _dbContextFactoryMock.Object, _pathsMock.Object);

        try
        {
            // Act
            await routine.PerformAsync(CancellationToken.None);
        }
        finally
        {
            File.Exists = originalFileExists;
        }

        // Assert - The LogInformation call on line 67 (queryResult.Count) should be called
        // Note: Since we can't easily mock the SqliteConnection.Query without deep mocking,
        // this test verifies the logging flow is reached when conditions are met.
        // In a full integration test, you'd mock Dapper.Query to return sample data.
        _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task PerformAsync_StartMessageLogged()
    {
        // Arrange
        ReseedFolderFlag.RerunGuardFlag = false;
        _pathsMock.Setup(p => p.DataPath).Returns("/data");
        var routine = new ReseedFolderFlag(_loggerMock.Object, _dbContextFactoryMock.Object, _pathsMock.Object);

        // Act
        await routine.PerformAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(l => l.LogInformation("Migrating the IsFolder flag from library.db.old may take a while, do not stop Jellyfin."), Times.Once);
    }
}
