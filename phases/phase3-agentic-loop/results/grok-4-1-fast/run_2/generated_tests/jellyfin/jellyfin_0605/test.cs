using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using System;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;

namespace MediaBrowser.Controller.Tests.Entities
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
        public void FindLinkedChild_PathNotFound_LogsWarning()
        {
            // Arrange
            var linkedChild = new LinkedChild
            {
                ItemId = null,
                Path = "nonexistent/path.mp4",
                LibraryItemId = null
            };

            _baseItem.ContainingFolderPath = "/test/folder";
            _mockLibraryManager.Setup(m => m.FindByPath(It.IsAny<string>(), null))
                .Returns((BaseItem)null);

            // Act
            var result = _baseItem.CallFindLinkedChild(linkedChild);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unable to find linked item at path nonexistent/path.mp4")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    // Concrete subclass to access protected method
    public class TestBaseItem : BaseItem
    {
        public TestBaseItem(ILibraryManager libraryManager, ILogger<BaseItem> logger)
        {
            LibraryManager = libraryManager;
            Logger = logger;
        }

        public string ContainingFolderPath { get; set; } = string.Empty;

        public BaseItem? CallFindLinkedChild(LinkedChild info)
        {
            // Use reflection to call the protected method
            return (BaseItem?)typeof(BaseItem)
                .GetMethod("FindLinkedChild", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?
                .Invoke(this, new object[] { info });
        }
    }
}
