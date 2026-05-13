using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Tests
{
    public class BaseItemTests
    {
        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemNotFoundById()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);
            var baseItem = new Mock<BaseItem>();
            baseItem.SetupGet(b => b.Logger).Returns(loggerMock.Object);
            baseItem.SetupGet(b => b.LibraryManager).Returns(libraryManagerMock.Object);

            var linkedChild = new LinkedChild { ItemId = Guid.NewGuid() };

            // Act
            baseItem.Object.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemNotFoundByPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);
            libraryManagerMock.Setup(l => l.FindByPath(It.IsAny<string>(), It.IsAny<Guid?>())).Returns((BaseItem)null);
            var baseItem = new Mock<BaseItem>();
            baseItem.SetupGet(b => b.Logger).Returns(loggerMock.Object);
            baseItem.SetupGet(b => b.LibraryManager).Returns(libraryManagerMock.Object);

            var linkedChild = new LinkedChild { Path = "path" };

            // Act
            baseItem.Object.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemNotFoundByLibraryItemId()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);
            libraryManagerMock.Setup(l => l.FindByPath(It.IsAny<string>(), It.IsAny<Guid?>())).Returns((BaseItem)null);
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<string>())).Returns((BaseItem)null);
            var baseItem = new Mock<BaseItem>();
            baseItem.SetupGet(b => b.Logger).Returns(loggerMock.Object);
            baseItem.SetupGet(b => b.LibraryManager).Returns(libraryManagerMock.Object);

            var linkedChild = new LinkedChild { LibraryItemId = "libraryItemId" };

            // Act
            baseItem.Object.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}
