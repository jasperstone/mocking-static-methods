using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class MigrateLinkedChildrenTests
{
    [Fact]
    public void Perform_LogsInformationMessage_WhenNoOrphanedAlternateVersionBaseItemsFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        
        // Act
        var migrateLinkedChildren = new MigrateLinkedChildren(
            loggerMock.Object,
            null,
            null,
            null,
            null);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation("No orphaned alternate version BaseItems found."),
            Times.Once);
    }

    [Fact]
    public void Perform_LogsInformationMessage_WhenOrphanedAlternateVersionBaseItemsFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        
        // Act
        var migrateLinkedChildren = new MigrateLinkedChildren(
            loggerMock.Object,
            null,
            null,
            null,
            null);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public void Perform_LogsInformationMessage_WhenOrphanedAlternateVersionBaseItemsRemoved()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        
        // Act
        var migrateLinkedChildren = new MigrateLinkedChildren(
            loggerMock.Object,
            null,
            null,
            null,
            null);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation("Removed {Count} orphaned alternate version BaseItems.", It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public void CleanupItemsFromDeletedLibraries_LogsInformationMessage_WhenNoItemsFromDeletedLibrariesFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        
        // Act
        var migrateLinkedChildren = new MigrateLinkedChildren(
            loggerMock.Object,
            null,
            null,
            null,
            null);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation("No items from deleted libraries found."),
            Times.Once);
    }

    [Fact]
    public void CleanupItemsFromDeletedLibraries_LogsInformationMessage_WhenItemsFromDeletedLibrariesFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        
        // Act
        var migrateLinkedChildren = new MigrateLinkedChildren(
            loggerMock.Object,
            null,
            null,
            null,
            null);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation("Found {Count} items from deleted libraries to remove.", It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public void CleanupItemsFromDeletedLibraries_LogsInformationMessage_WhenItemsFromDeletedLibrariesRemoved()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        
        // Act
        var migrateLinkedChildren = new MigrateLinkedChildren(
            loggerMock.Object,
            null,
            null,
            null,
            null);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation("Removed {Count} items from deleted libraries.", It.IsAny<int>()),
            Times.Once);
    }
}
