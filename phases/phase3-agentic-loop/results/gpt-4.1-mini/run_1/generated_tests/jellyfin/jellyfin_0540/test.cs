using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsStartingMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>()).Returns(loggerMock.Object);

            var orphanedTopParentId = Guid.NewGuid();

            var baseItems = new List<BaseItemEntity>
            {
                // Orphaned item: TopParentId points to a non-existent library
                new BaseItemEntity
                {
                    Id = Guid.NewGuid(),
                    TopParentId = orphanedTopParentId,
                    Type = "SomeType"
                }
            }.AsQueryable();

            var baseItemsDbSetMock = CreateDbSetMock(baseItems);

            var linkedChildren = new List<LinkedChildEntity>().AsQueryable();
            var linkedChildrenDbSetMock = CreateDbSetMock(linkedChildren);

            var dbContextMock = new Mock<JellyfinDbContext>(MockBehavior.Strict, null, null, null, null);
            dbContextMock.Setup(c => c.BaseItems).Returns(baseItemsDbSetMock.Object);
            dbContextMock.Setup(c => c.LinkedChildren).Returns(linkedChildrenDbSetMock.Object);

            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbContextFactoryMock.Setup(f => f.CreateDbContext()).Returns(dbContextMock.Object);

            var libraryManagerMock = new Mock<ILibraryManager>();

            var migrate = (MigrateLinkedChildren)Activator.CreateInstance(
                typeof(MigrateLinkedChildren),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new object[]
                {
                    loggerFactoryMock.Object,
                    dbContextFactoryMock.Object,
                    libraryManagerMock.Object,
                    new object(),
                    new object()
                },
                null);

            // Act
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(migrate, new object[] { dbContextMock.Object });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting cleanup of items from deleted libraries...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private static Mock<DbSet<T>> CreateDbSetMock<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }
}
