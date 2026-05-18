using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using Jellyfin.Controller; // Correct placement of using directive
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("TestProj")] // Ensure the test project can access internal types

public class MigrateLinkedChildrenTests
{
    private readonly Mock<ILogger<MigrateLinkedChildren>> _loggerMock;
    private readonly Mock<ILibraryManager> _libraryManagerMock;
    private readonly Mock<JellyfinDbContext> _contextMock;

    public MigrateLinkedChildrenTests()
    {
        _loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
        _libraryManagerMock = new Mock<ILibraryManager>();
        _contextMock = new Mock<JellyfinDbContext>();
    }

    [Fact]
    public void CleanupOrphanedAlternateVersions_NoOrphanedVersions_LogsNoOrphanedVersionsMessage()
    {
        // Arrange
        var baseItems = new List<BaseItem>
        {
            new BaseItem { Id = 1, OwnerId = null, ExtraType = null }
        };
        _contextMock.Setup(c => c.BaseItems).Returns(baseItems.AsQueryable());
        _contextMock.Setup(c => c.LinkedChildren).Returns(Enumerable.Empty<LinkedChildEntity>().AsQueryable());

        var routine = new MigrateLinkedChildren(
            Mock.Of<ILoggerFactory>(),
            Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
            _libraryManagerMock.Object,
            Mock.Of<IServerApplicationHost>(),
            Mock.Of<IServerApplicationPaths>()
        );

        // Act
        routine.CleanupOrphanedAlternateVersions(_contextMock.Object);

        // Assert
        _loggerMock.Verify(l => l.LogInformation("No orphaned alternate version BaseItems found."), Times.Once);
        _loggerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void CleanupOrphanedAlternateVersions_OrphanedVersionsFound_LogsCorrectMessages()
    {
        // Arrange
        var baseItems = new List<BaseItem>
        {
            new BaseItem { Id = 1, OwnerId = 1, ExtraType = null },
            new BaseItem { Id = 2, OwnerId = 2, ExtraType = null }
        };
        _contextMock.Setup(c => c.BaseItems).Returns(baseItems.AsQueryable());
        _contextMock.Setup(c => c.LinkedChildren).Returns(Enumerable.Empty<LinkedChildEntity>().AsQueryable());

        var routine = new MigrateLinkedChildren(
            Mock.Of<ILoggerFactory>(),
            Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
            _libraryManagerMock.Object,
            Mock.Of<IServerApplicationHost>(),
            Mock.Of<IServerApplicationPaths>()
        );

        // Act
        routine.CleanupOrphanedAlternateVersions(_contextMock.Object);

        // Assert
        _loggerMock.Verify(l => l.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", 2), Times.Once);
        _loggerMock.Verify(l => l.LogInformation("Removed {Count} orphaned alternate version BaseItems.", 2), Times.Once);
        _loggerMock.VerifyNoOtherCalls();
    }
}
