using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenTests
    {
        private readonly Mock<ILogger<MigrateLinkedChildren>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly MigrateLinkedChildren _migrateLinkedChildren;

        public MigrateLinkedChildrenTests()
        {
            _loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            _libraryManagerMock = new Mock<ILibraryManager>();

            _migrateLinkedChildren = new MigrateLinkedChildren(
                new LoggerFactory().AddProvider(new MockLoggerProvider(_loggerMock.Object)),
                null, // Mocked out for this test
                _libraryManagerMock.Object,
                null, // Mocked out for this test
                null  // Mocked out for this test
            );
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_NoItemsFound_LogsNoItemsFound()
        {
            // Arrange
            _libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);

            // Act
            _migrateLinkedChildren.CleanupItemsFromDeletedLibraries(null);

            // Assert
            _loggerMock.Verify(
                l => l.LogInformation("No items from deleted libraries found."),
                Times.Once);
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_ItemsFound_LogsItemsFoundAndRemoved()
        {
            // Arrange
            var orphanedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var items = new List<BaseItem> { new BaseItem(), new BaseItem() };

            _libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>()))
                .Returns((BaseItem)null)
                .Callback<Guid>(id => items.Add(new BaseItem()));

            _libraryManagerMock.Setup(m => m.DeleteItemsUnsafeFast(It.IsAny<List<BaseItem>>()))
                .Callback<List<BaseItem>>(itemsToDelete => items.Clear());

            // Act
            _migrateLinkedChildren.CleanupItemsFromDeletedLibraries(null);

            // Assert
            _loggerMock.Verify(
                l => l.LogInformation("Found {Count} items from deleted libraries to remove.", orphanedIds.Count),
                Times.Once);

            _loggerMock.Verify(
                l => l.LogInformation("Removed {Count} items from deleted libraries.", items.Count),
                Times.Once);
        }
    }

    public class BaseItem
    {
        public Guid? TopParentId { get; set; }
    }

    public interface ILibraryManager
    {
        BaseItem GetItemById(Guid id);
        void DeleteItemsUnsafeFast(List<BaseItem> items);
    }

    public class MockLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public MockLoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { }
    }
}
