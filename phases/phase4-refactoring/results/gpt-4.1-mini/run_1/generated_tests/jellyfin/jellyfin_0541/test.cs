using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Database.Implementations;
using System.Reflection;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsNoItemsFound_WhenNoOrphanedItems()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>()).Returns(loggerMock.Object);

            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var appPathsMock = new Mock<IServerApplicationPaths>();

            // Create an in-memory DbContext with no BaseItems
            var options = new DbContextOptionsBuilder<JellyfinDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var context = new JellyfinDbContext(options);

            dbContextFactoryMock.Setup(f => f.CreateDbContext()).Returns(context);

            var migration = (MigrateLinkedChildren)Activator.CreateInstance(
                typeof(MigrateLinkedChildren),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new object[] { loggerFactoryMock.Object, dbContextFactoryMock.Object, libraryManagerMock.Object, appHostMock.Object, appPathsMock.Object },
                null)!;

            // Use reflection to invoke private method CleanupItemsFromDeletedLibraries
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);

            // Act
            method.Invoke(migration, new object[] { context });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No items from deleted libraries found.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
