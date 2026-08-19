using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Controller.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemNotFoundById()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var baseItem = new MediaBrowser.Controller.Entities.Movie();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            libraryManagerMock.Setup(x => x.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);
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
            var loggerMock = new Mock<ILogger>();
            var baseItem = new MediaBrowser.Controller.Entities.Movie();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            libraryManagerMock.Setup(x => x.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);
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
            var loggerMock = new Mock<ILogger>();
            var baseItem = new MediaBrowser.Controller.Entities.Movie();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            libraryManagerMock.Setup(x => x.GetItemById(It.IsAny<Guid>())).Returns((BaseItem)null);
            var linkedChild = new LinkedChild { LibraryItemId = "libraryItemId" };

            // Act
            baseItem.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("Unable to find linked item by LibraryItemId {0}", linkedChild.LibraryItemId), Times.Once);
        }
    }
}
