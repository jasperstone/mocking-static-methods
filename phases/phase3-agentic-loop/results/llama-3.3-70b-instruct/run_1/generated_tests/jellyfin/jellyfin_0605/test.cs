using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace MediaBrowser.Controller.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemNotFoundById()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);
            var baseItem = new Mock<BaseItem>();
            baseItem.Setup(b => b.LibraryManager).Returns(libraryManagerMock.Object);
            baseItem.Setup(b => b.Logger).Returns(loggerMock.Object);

            var linkedChild = new LinkedChild { ItemId = Guid.NewGuid() };

            // Act
            baseItem.Object.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to find linked item by ItemId {0}", linkedChild.ItemId), Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemNotFoundByPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);
            libraryManagerMock.Setup(l => l.FindByPath(It.IsAny<string>(), It.IsAny<string>())).Returns((BaseItem)null);
            var baseItem = new Mock<BaseItem>();
            baseItem.Setup(b => b.LibraryManager).Returns(libraryManagerMock.Object);
            baseItem.Setup(b => b.Logger).Returns(loggerMock.Object);

            var linkedChild = new LinkedChild { Path = "path" };

            // Act
            baseItem.Object.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to find linked item at path {0}", linkedChild.Path), Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemNotFoundByLibraryItemId()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);
            libraryManagerMock.Setup(l => l.FindByPath(It.IsAny<string>(), It.IsAny<string>())).Returns((BaseItem)null);
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<string>())).Returns((BaseItem)null);
            var baseItem = new Mock<BaseItem>();
            baseItem.Setup(b => b.LibraryManager).Returns(libraryManagerMock.Object);
            baseItem.Setup(b => b.Logger).Returns(loggerMock.Object);

            var linkedChild = new LinkedChild { LibraryItemId = "libraryItemId" };

            // Act
            baseItem.Object.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Unable to find linked item by LibraryItemId {0}", linkedChild.LibraryItemId), Times.Once);
        }
    }
}
