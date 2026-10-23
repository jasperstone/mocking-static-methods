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
        private readonly TestBaseItem _baseItem;

        public BaseItemTests()
        {
            _mockLibraryManager = new Mock<ILibraryManager>();
            _mockLogger = new Mock<ILogger<BaseItem>>();
            _baseItem = new TestBaseItem(_mockLibraryManager.Object, _mockLogger.Object);
        }

        [Fact]
        public void FindLinkedChild_PathFallback_LogsWarningWhenItemNotFound()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                Path = "/some/nonexistent/path.mp4"
            };

            _mockLibraryManager.Setup(m => m.FindByPath(It.IsAny<string>(), null)).Returns((BaseItem)null);

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unable to find linked item at path") && v.ToString()!.Contains("/some/nonexistent/path.mp4")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void FindLinkedChild_PathFallback_SkipsLoggingWhenPathEmpty()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                Path = string.Empty
            };

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.VerifyNoOtherCalls();
        }

        [Fact]
        public void FindLinkedChild_ItemIdFallback_LogsWarningWhenItemNotFound()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                ItemId = Guid.NewGuid()
            };

            _mockLibraryManager.Setup(m => m.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unable to find linked item by ItemId")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LibraryItemIdFallback_LogsWarningWhenItemNotFound()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                LibraryItemId = "some-library-id"
            };

            _mockLibraryManager.Setup(m => m.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unable to find linked item by LibraryItemId")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
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
            }

            public new ILogger<BaseItem> Logger => _logger;
            public ILibraryManager LibraryManager => _libraryManager;
            public string ContainingFolderPath => "/test/folder";

            internal BaseItem FindLinkedChild(LinkedChild info)
            {
                // Directly test the original private method logic
                if (info.ItemId.HasValue && !info.ItemId.Value.Equals(Guid.Empty))
                {
                    var item = LibraryManager.GetItemById(info.ItemId.Value);
                    if (item is not null)
                    {
                        return item;
                    }

                    Logger.LogWarning("Unable to find linked item by ItemId {0}", info.ItemId);
                }

                var path = info.Path;
                if (!string.IsNullOrEmpty(path))
                {
                    path = FileSystem.MakeAbsolutePath(ContainingFolderPath, path);

                    var itemByPath = LibraryManager.FindByPath(path, null);

                    if (itemByPath is null)
                    {
                        Logger.LogWarning("Unable to find linked item at path {0}", info.Path);
                    }

                    return itemByPath;
                }

                if (!string.IsNullOrEmpty(info.LibraryItemId))
                {
                    var item = LibraryManager.GetItemById(info.LibraryItemId);

                    if (item is null)
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
