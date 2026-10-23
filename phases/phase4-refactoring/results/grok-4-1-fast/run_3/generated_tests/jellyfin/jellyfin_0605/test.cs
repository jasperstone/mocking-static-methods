using System;
using System.IO;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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

            _baseItem = new TestBaseItem(_mockLibraryManager.Object, _mockLogger.Object)
            {
                ContainingFolderPath = "/test/folder"
            };
        }

        [Fact]
        public void FindLinkedChild_NonEmptyPathNotFound_LogsPathWarning()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                Path = "nonexistent/path.mp4"
            };

            var absolutePath = Path.Combine(_baseItem.ContainingFolderPath, linkedChild.Path);
            _mockLibraryManager.Setup(m => m.FindByPath(absolutePath, null)).Returns((BaseItem)null);

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    (Exception)null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void FindLinkedChild_PathFound_ReturnsItemNoWarning()
        {
            // Arrange
            var expectedItem = new Mock<BaseItem>().Object;
            var linkedChild = new LinkedChild
            {
                Path = "existing/path.mp4"
            };

            var absolutePath = Path.Combine(_baseItem.ContainingFolderPath, linkedChild.Path);
            _mockLibraryManager.Setup(m => m.FindByPath(absolutePath, null)).Returns(expectedItem);

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Same(expectedItem, result);
            _mockLogger.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void FindLinkedChild_EmptyPath_DoesNotLogPathWarning()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                Path = ""
            };

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        private class TestBaseItem : BaseItem
        {
            public string ContainingFolderPath { get; set; } = string.Empty;
            public ILibraryManager LibraryManager { get; }
            public ILogger<BaseItem> Logger { get; }

            public TestBaseItem(ILibraryManager libraryManager, ILogger<BaseItem> logger)
            {
                LibraryManager = libraryManager;
                Logger = logger;
            }

            public virtual BaseItem? FindLinkedChild(LinkedChild info)
            {
                if (info.ItemId.HasValue && !info.ItemId.Value.Equals(Guid.Empty))
                {
                    var item = LibraryManager.GetItemById(info.ItemId.Value);
                    if (item != null)
                    {
                        return item;
                    }
                    Logger.LogWarning("Unable to find linked item by ItemId {0}", info.ItemId);
                }

                var path = info.Path;
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.Combine(ContainingFolderPath, path);
                    var itemByPath = LibraryManager.FindByPath(path, null);
                    if (itemByPath == null)
                    {
                        Logger.LogWarning("Unable to find linked item at path {0}", info.Path);
                    }
                    return itemByPath;
                }

                if (!string.IsNullOrEmpty(info.LibraryItemId))
                {
                    var item = LibraryManager.GetItemById(info.LibraryItemId);
                    if (item == null)
                    {
                        Logger.LogWarning("Unable to find linked item by LibraryItemId {0}", info.LibraryItemId);
                    }
                    return item;
                }

                return null;
            }
        }
    }
}
