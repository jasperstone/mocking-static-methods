using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;

public class MigrateLinkedChildrenTests
{
    [Fact]
    public void Perform_LogsNoOrphanedAlternateVersionBaseItemsFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
        var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var libraryManagerMock = new Mock<ILibraryManager>();
        var appHostMock = new Mock<IServerApplicationHost>();
        var appPathsMock = new Mock<IServerApplicationPaths>();

        var migrateLinkedChildren = new MigrateLinkedChildren(
            loggerMock.Object,
            dbProviderMock.Object,
            libraryManagerMock.Object,
            appHostMock.Object,
            appPathsMock.Object);

        // Act
        migrateLinkedChildren.Perform();

        // Assert
        loggerMock.Verify(l => l.LogInformation("No orphaned alternate version BaseItems found."), Times.Once);
    }

    [Fact]
    public void Perform_LogsFoundOrphanedAlternateVersionBaseItemsToRemove()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
        var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var libraryManagerMock = new Mock<ILibraryManager>();
        var appHostMock = new Mock<IServerApplicationHost>();
        var appPathsMock = new Mock<IServerApplicationPaths>();

        var migrateLinkedChildren = new MigrateLinkedChildren(
            loggerMock.Object,
            dbProviderMock.Object,
            libraryManagerMock.Object,
            appHostMock.Object,
            appPathsMock.Object);

        // Act
        migrateLinkedChildren.Perform();

        // Assert
        loggerMock.Verify(l => l.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", 1), Times.Once);
    }

    [Fact]
    public void Perform_LogsRemovedOrphanedAlternateVersionBaseItems()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
        var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var libraryManagerMock = new Mock<ILibraryManager>();
        var appHostMock = new Mock<IServerApplicationHost>();
        var appPathsMock = new Mock<IServerApplicationPaths>();

        var migrateLinkedChildren = new MigrateLinkedChildren(
            loggerMock.Object,
            dbProviderMock.Object,
            libraryManagerMock.Object,
            appHostMock.Object,
            appPathsMock.Object);

        // Act
        migrateLinkedChildren.Perform();

        // Assert
        loggerMock.Verify(l => l.LogInformation("Removed {Count} orphaned alternate version BaseItems.", 1), Times.Once);
    }
}
