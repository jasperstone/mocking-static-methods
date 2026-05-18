using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class BaseItemTests
    {
        private readonly Mock<ILibraryManager> _mockLibraryManager;
        private readonly Mock<ILogger<BaseItem>> _mockLogger;
        private readonly TestBaseItem _baseItem;

        public BaseItemTests()
        {
            _mockLibraryManager = new Mock<ILibraryManager>();
            _mockLogger = new Mock<ILogger<BaseItem>>();
            _mockLibraryManager.Setup(m => m.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);
            _mockLibraryManager.Setup(m => m.GetItemById(It.IsAny<string>())).Returns((BaseItem)null);

            _baseItem = new TestBaseItem(_mockLibraryManager.Object, _mockLogger.Object)
            {
                ContainingFolderPath = "/test/folder",
                Id = Guid.NewGuid(),
                Name = "Test Item",
                Path = string.Empty
            };
        }

        [Fact]
        public void FindLinkedChild_PathSearch_LogsWarningWhenItemNotFound()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                Path = "/nonexistent/path.mp4"
            };

            _mockLibraryManager.Setup(m => m.FindByPath("/test/folder/nonexistent/path.mp4", null))
                .Returns((BaseItem)null);

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.LogWarning("Unable to find linked item at path {0}", linkedChild.Path),
                Times.Once);
        }

        [Fact]
        public void FindLinkedChild_PathSearch_DoesNotLogWhenItemFound()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                Path = "/existing/path.mp4"
            };
            var mockItem = new Mock<BaseItem>().Object;

            _mockLibraryManager.Setup(m => m.FindByPath("/test/folder/existing/path.mp4", null))
                .Returns(mockItem);

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.NotNull(result);
            _mockLogger.Verify(
                x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<Exception>()),
                Times.Never);
        }

        [Fact]
        public void FindLinkedChild_ItemIdSearch_LogsWarningWhenItemNotFound()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                ItemId = Guid.NewGuid()
            };

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.LogWarning("Unable to find linked item by ItemId {0}", linkedChild.ItemId),
                Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LibraryItemIdSearch_LogsWarningWhenItemNotFound()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                LibraryItemId = "test-library-id"
            };

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.LogWarning("Unable to find linked item by LibraryItemId {0}", linkedChild.LibraryItemId),
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
                Id = Guid.NewGuid();
                Name = "Test Item";
                Path = string.Empty;
            }

            public string ContainingFolderPath { get; set; } = string.Empty;

            // Expose protected method for testing
            public new BaseItem? FindLinkedChild(LinkedChild info) => base.FindLinkedChild(info);

            protected override ILogger Logger => _logger;
            protected override ILibraryManager LibraryManager => _libraryManager;
        }
    }
}
