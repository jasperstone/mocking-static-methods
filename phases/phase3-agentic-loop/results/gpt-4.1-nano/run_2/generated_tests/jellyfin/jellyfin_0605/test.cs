using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.Controller.Entities;
using System;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class BaseItemTests
    {
        private class TestBaseItem : BaseItem
        {
            public TestBaseItem() : base() { }
        }

        [Fact]
        public void FindLinkedChild_Should_LogWarning_When_ItemId_NotFound()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var itemId = Guid.NewGuid();
            var linkedChild = new LinkedChild { ItemId = itemId, Path = "somepath", LibraryItemId = "libid" };

            var item = new TestBaseItem();
            item.LibraryManager = mockLibraryManager.Object;
            item.Logger = mockLogger.Object;

            mockLibraryManager.Setup(m => m.GetItemById(itemId)).Returns((BaseItem)null);
            mockLibraryManager.Setup(m => m.FindByPath(It.IsAny<string>(), null)).Returns((BaseItem)null);
            mockLibraryManager.Setup(m => m.GetItemById(It.IsAny<string>())).Returns((BaseItem)null);

            // Act
            var result = item.FindLinkedChild(linkedChild);

            // Assert
            mockLogger.Verify(
                x => x.LogWarning("Unable to find linked item by ItemId {0}", itemId),
                Times.Once);
            Assert.Null(result);
        }
    }
}
