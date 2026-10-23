using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using MediaBrowser.Controller.Library;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller;
using Jellyfin.Database.Implementations.Entities;

public class MigrateLinkedChildrenTests
{
    [Fact]
    public void CleanupItemsFromDeletedLibraries_NoItemsFound_LogsCorrectly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
        var dbContextMock = new Mock<JellyfinDbContext>();
        var libraryManagerMock = new Mock<ILibraryManager>();

        var migrateLinkedChildren = new MigrateLinkedChildren(
            Mock.Of<ILoggerFactory>(),
            Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
            libraryManagerMock.Object,
            Mock.Of<IServerApplicationHost>(),
            Mock.Of<IServerApplicationPaths>()
        );

        dbContextMock.Setup(db => db.BaseItems).Returns(new List<BaseItem>().AsQueryable().BuildMockDbSet());

        // Act
        migrateLinkedChildren.CleanupItemsFromDeletedLibraries(dbContextMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No items from deleted libraries found.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
