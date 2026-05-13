using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Tests
{
    public class BaseItemTests
    {
        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemNotFoundById()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var baseItem = new BaseItem();
            baseItem.Logger = loggerMock.Object;
            var linkedChild = new LinkedChild { ItemId = Guid.NewGuid() };

            // Act
            baseItem.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemNotFoundByPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var baseItem = new BaseItem();
            baseItem.Logger = loggerMock.Object;
            var linkedChild = new LinkedChild { Path = "path" };

            // Act
            baseItem.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemNotFoundByLibraryItemId()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var baseItem = new BaseItem();
            baseItem.Logger = loggerMock.Object;
            var linkedChild = new LinkedChild { LibraryItemId = "libraryItemId" };

            // Act
            baseItem.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}
