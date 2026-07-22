using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Jellyfin.Tests.Migrations.Routines
{
    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void LogInformation_NoOrphanedBaseItems_ShouldLogNoOrphaned()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var appPathsMock = new Mock<IServerApplicationPaths>();

            // Setup DbContext factory to return a context with no orphaned BaseItems
            dbFactoryMock.Setup(f => f.CreateDbContext()).Returns(() =>
            {
                var options = new DbContextOptionsBuilder<JellyfinDbContext>()
                    .Options;
                var context = new JellyfinDbContext(options);
                // Seed with no orphaned BaseItems
                context.BaseItems.AddRange(new List<BaseItem>());
                context.SaveChanges();
                return context;
            });

            var routine = new MigrateLinkedChildren(
                Mock.Of<ILoggerFactory>(),
                dbFactoryMock.Object,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object);

            // Act
            routine.Perform();

            // Assert
            // Verify that LogInformation was called with the message about no orphaned BaseItems
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No orphaned alternate version BaseItems found.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
