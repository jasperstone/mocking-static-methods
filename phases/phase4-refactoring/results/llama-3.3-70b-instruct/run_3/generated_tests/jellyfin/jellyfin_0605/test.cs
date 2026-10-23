using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

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
            var baseItem = new Folder();
            baseItem.Logger = loggerMock.Object;
            baseItem.LibraryManager = libraryManagerMock.Object;
            var linkedChild = new LinkedChild { ItemId = Guid.NewGuid() };

            // Act
            baseItem.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("Unable to find linked item by ItemId {0}", linkedChild.ItemId), Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemNotFoundByPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            libraryManagerMock.Setup(l => l.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);
            libraryManagerMock.Setup(l => l.FindByPath(It.IsAny<string>(), It.IsAny<string>())).Returns((BaseItem)null);
            var baseItem = new Folder();
            baseItem.Logger = loggerMock.Object;
            baseItem.LibraryManager = libraryManagerMock.Object;
            var linkedChild = new LinkedChild { Path = "path" };

            // Act
            baseItem.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("Unable to find linked item at path {0}", linkedChild.Path), Times.Once);
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
            var baseItem = new Folder();
            baseItem.Logger = loggerMock.Object;
            baseItem.LibraryManager = libraryManagerMock.Object;
            var linkedChild = new LinkedChild { LibraryItemId = "libraryItemId" };

            // Act
            baseItem.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("Unable to find linked item by LibraryItemId {0}", linkedChild.LibraryItemId), Times.Once);
        }
    }
}
