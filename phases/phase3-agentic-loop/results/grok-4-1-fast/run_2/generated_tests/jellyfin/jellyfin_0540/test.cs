using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateLinkedChildrenTests
    {
        private readonly Mock<ILogger<MigrateLinkedChildren>> _loggerMock;

        public MigrateLinkedChildrenTests()
        {
            _loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            _loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsStartingMessage()
        {
            // Arrange
            var mockContext = new Mock<JellyfinDbContext>();
            var migration = new TestableMigrateLinkedChildren(_loggerMock.Object, mockContext.Object);

            // Act
            migration.CallCleanupItemsFromDeletedLibraries();

            // Assert - specifically test line 324 LogInformation call
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(v => v.ToString()!.Contains("Starting cleanup of items from deleted libraries...")),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_NoOrphanedItems_LogsNoItemsFound()
        {
            // Arrange
            var mockContext = new Mock<JellyfinDbContext>();
            var baseItemsMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<BaseItemEntity>>();
            baseItemsMock.Setup(b => b.Where(It.IsAny<Expression<Func<BaseItemEntity, bool>>>())
                .Where(It.IsAny<Expression<Func<BaseItemEntity, bool>>>())
                .Select(It.IsAny<Expression<Func<BaseItemEntity, Guid>>>())
                .ToList())
                .Returns(new List<Guid>());
            mockContext.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);
            
            var migration = new TestableMigrateLinkedChildren(_loggerMock.Object, mockContext.Object);

            // Act
            migration.CallCleanupItemsFromDeletedLibraries();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(v => v.ToString()!.Contains("No items from deleted libraries found.")),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Testable subclass to access private method and avoid dependency injection issues
    internal class TestableMigrateLinkedChildren : MigrateLinkedChildren
    {
        public TestableMigrateLinkedChildren(ILogger<MigrateLinkedChildren> logger, JellyfinDbContext context)
            : base(
                Mock.Of<ILoggerFactory>(),
                Mock.Of<IDbContextFactory<JellyfinDbContext>>(),
                Mock.Of<ILibraryManager>(),
                Mock.Of<IServerApplicationHost>(),
                Mock.Of<IServerApplicationPaths>())
        {
            // Use provided logger and context for testing
            typeof(MigrateLinkedChildren).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(this, logger);
        }

        public void CallCleanupItemsFromDeletedLibraries()
        {
            var context = new Mock<JellyfinDbContext>().Object;
            CleanupItemsFromDeletedLibraries(context);
        }
    }
}
