using System;
using System.IO;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemTests
    {
        private readonly Mock<ILibraryManager> _mockLibraryManager;
        private readonly Mock<ILogger<BaseItem>> _mockLogger;
        private readonly BaseItem _baseItem;

        public BaseItemTests()
        {
            _mockLibraryManager = new Mock<ILibraryManager>();
            _mockLogger = new Mock<ILogger<BaseItem>>();

            // Create a concrete subclass to access the private method
            _baseItem = new TestBaseItem(_mockLibraryManager.Object, _mockLogger.Object);
        }

        [Fact]
        public void FindLinkedChild_WithNonEmptyPath_ReturnsNull_LogsWarning()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                Path = "/some/path/to/item"
            };

            _mockLibraryManager.Setup(m => m.FindByPath(It.IsAny<string>(), null)).Returns((BaseItem)null);

            // Act
            var result = ((TestBaseItem)_baseItem).FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                l => l.LogWarning(
                    "Unable to find linked item at path {0}",
                    linkedChild.Path),
                Times.Once);
        }

        [Fact]
        public void FindLinkedChild_WithItemIdNotFound_LogsWarning()
        {
            // Arrange
            var itemId = Guid.NewGuid();
            var linkedChild = new LinkedChild
            {
                ItemId = itemId
            };

            _mockLibraryManager.Setup(m => m.GetItemById(itemId)).Returns((BaseItem)null);

            // Act
            var result = ((TestBaseItem)_baseItem).FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                l => l.LogWarning(
                    "Unable to find linked item by ItemId {0}",
                    itemId),
                Times.Once);
        }

        [Fact]
        public void FindLinkedChild_WithLibraryItemIdNotFound_LogsWarning()
        {
            // Arrange
            var libraryItemId = "some-library-id";
            var linkedChild = new LinkedChild
            {
                LibraryItemId = libraryItemId
            };

            _mockLibraryManager.Setup(m => m.GetItemById(libraryItemId)).Returns((BaseItem)null);

            // Act
            var result = ((TestBaseItem)_baseItem).FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                l => l.LogWarning(
                    "Unable to find linked item by LibraryItemId {0}",
                    libraryItemId),
                Times.Once);
        }

        private class TestBaseItem : BaseItem
        {
            private readonly ILibraryManager _libraryManager;
            private readonly ILogger<BaseItem> _logger;

            public TestBaseItem(ILibraryManager libraryManager, ILogger<BaseItem> logger)
            {
                _libraryManager = libraryManager;
                _logger = logger;
            }

            public ILibraryManager LibraryManager 
            { 
                get => _libraryManager; 
            }

            public string ContainingFolderPath => "/base/path";

            // Don't override Logger - use reflection or accept the real logger behavior
            internal BaseItem FindLinkedChild(LinkedChild info) => (BaseItem?)typeof(BaseItem)
                .GetMethod("FindLinkedChild", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(this, new object[] { info })!;
        }
    }
}
