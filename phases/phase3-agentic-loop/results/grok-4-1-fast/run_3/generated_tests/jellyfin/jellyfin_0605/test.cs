using System;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class BaseItemTests
    {
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<ILogger<BaseItem>> _loggerMock;
        private readonly TestBaseItem _baseItem;

        public BaseItemTests()
        {
            _libraryManagerMock = new Mock<ILibraryManager>();
            _loggerMock = new Mock<ILogger<BaseItem>>();
            _baseItem = new TestBaseItem(_loggerMock.Object, _libraryManagerMock.Object)
            {
                ContainingFolderPath = "/test/folder"
            };
        }

        [Fact]
        public void FindLinkedChild_PathNotFound_LogsWarningWithOriginalPath()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                ItemId = null,
                Path = "relative/path/to/missing/item.mkv"
            };

            _libraryManagerMock
                .Setup(m => m.FindByPath(It.IsAny<string>(), null))
                .Returns((BaseItem)null);

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(
                        formatter => formatter(null, null)!.Contains("Unable to find linked item at path") &&
                                    formatter(null, null)!.Contains("relative/path/to/missing/item.mkv"))),
                Times.Once);
        }

        [Fact]
        public void FindLinkedChild_PathFound_NoWarningLogged()
        {
            // Arrange
            var linkedChild = new LinkedChild { Path = "relative/path/to/found/item.mkv" };
            var foundItem = new TestBaseItem(_loggerMock.Object, _libraryManagerMock.Object);

            _libraryManagerMock
                .Setup(m => m.FindByPath(It.IsAny<string>(), null))
                .Returns(foundItem);

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Equal(foundItem, result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        // Test subclass to access protected method
        private class TestBaseItem : BaseItem
        {
            public TestBaseItem(ILogger<BaseItem> logger, ILibraryManager libraryManager)
            {
                Logger = logger;
                LibraryManager = libraryManager;
            }

            public string ContainingFolderPath { get; set; } = string.Empty;

            public new BaseItem? FindLinkedChild(LinkedChild info)
            {
                return base.FindLinkedChild(info);
            }
        }
    }
}
