using System;
using System.IO;
using MediaBrowser.Controller.Library;
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

            // Create a concrete subclass of BaseItem to test the protected method
            _baseItem = new TestBaseItem(_mockLibraryManager.Object, _mockLogger.Object)
            {
                ContainingFolderPath = "/test/folder"
            };
        }

        [Fact]
        public void FindLinkedChild_PathSearch_FindsItem_ReturnsItem()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                ItemId = null,
                Path = "relative/path/to/item.mp4"
            };
            var expectedItem = new Mock<BaseItem>().Object;
            var absolutePath = Path.Combine(_baseItem.ContainingFolderPath, linkedChild.Path);

            _mockLibraryManager.Setup(m => m.FindByPath(absolutePath, false)).Returns(expectedItem);

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Equal(expectedItem, result);
            _mockLogger.VerifyNoInteractions();
        }

        [Fact]
        public void FindLinkedChild_PathSearch_Fails_LogsWarning()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                ItemId = null,
                Path = "relative/path/to/missing/item.mp4"
            };
            var absolutePath = Path.Combine(_baseItem.ContainingFolderPath, linkedChild.Path);

            _mockLibraryManager.Setup(m => m.FindByPath(absolutePath, false)).Returns((BaseItem)null);

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t == "Unable to find linked item at path {0}" && v.ToString().Contains(linkedChild.Path)),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }

        [Fact]
        public void FindLinkedChild_PathNull_SkipsPathSearch_NoLog()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                ItemId = null,
                Path = null
            };

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.VerifyNoInteractions();
        }

        [Fact]
        public void FindLinkedChild_PathEmpty_SkipsPathSearch_NoLog()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                ItemId = null,
                Path = ""
            };

            // Act
            var result = _baseItem.FindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.VerifyNoInteractions();
        }

        // Test subclass to access protected method and provide required dependencies
        private class TestBaseItem : BaseItem
        {
            public TestBaseItem(ILibraryManager libraryManager, ILogger<BaseItem> logger)
            {
                LibraryManager = libraryManager;
                Logger = logger;
            }

            public string ContainingFolderPath { get; set; } = string.Empty;

            public new BaseItem? FindLinkedChild(LinkedChild info) => base.FindLinkedChild(info);
        }
    }
}
