using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Tests.Migrations
{
    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void LogInformation_Called_When_NoOrphanedItems()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>()).Returns(loggerMock.Object);

            var options = new DbContextOptionsBuilder<JellyfinDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_NoOrphanedItems")
                .Options;

            using var context = new JellyfinDbContext(options);
            var libraryManagerMock = new Mock<ILibraryManager>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var appPathsMock = new Mock<IServerApplicationPaths>();

            var routine = new MigrateLinkedChildren(
                loggerFactoryMock.Object,
                null,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object);

            // Act
            routine.Perform();

            // Assert
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
