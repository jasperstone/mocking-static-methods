using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using System;

namespace MediaBrowser.Tests.Controller.Entities
{
    public class TestableBaseItem : BaseItem
    {
        public TestableBaseItem()
        {
            Logger = Mock.Of<ILogger<BaseItem>>();
            LibraryManager = Mock.Of<ILibraryManager>();
        }
    }

    public class BaseItemTests
    {
        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemIdNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var baseItem = new TestableBaseItem
            {
                Logger = loggerMock.Object,
                LibraryManager = libraryManagerMock.Object
            };

            var linkedChild = new LinkedChild
            {
                ItemId = Guid.NewGuid(),
                Path = null,
                LibraryItemId = null
            };

            libraryManagerMock.Setup(m => m.GetItemById(linkedChild.ItemId.Value)).Returns((BaseItem)null);

            // Act
            baseItem.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Unable to find linked item by ItemId {0}")), linkedChild.ItemId), Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenPathNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var baseItem = new TestableBaseItem
            {
                Logger = loggerMock.Object,
                LibraryManager = libraryManagerMock.Object
            };

            var linkedChild = new LinkedChild
            {
                ItemId = null,
                Path = "some/path",
                LibraryItemId = null
            };

            libraryManagerMock.Setup(m => m.FindByPath(linkedChild.Path, null)).Returns((BaseItem)null);

            // Act
            baseItem.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Unable to find linked item at path {0}")), linkedChild.Path), Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenLibraryItemIdNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var baseItem = new TestableBaseItem
            {
                Logger = loggerMock.Object,
                LibraryManager = libraryManagerMock.Object
            };

            var linkedChild = new LinkedChild
            {
                ItemId = null,
                Path = null,
                LibraryItemId = "some-id"
            };

            libraryManagerMock.Setup(m => m.GetItemById(linkedChild.LibraryItemId)).Returns((BaseItem)null);

            // Act
            baseItem.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Unable to find linked item by LibraryItemId {0}")), linkedChild.LibraryItemId), Times.Once);
        }
    }
}
