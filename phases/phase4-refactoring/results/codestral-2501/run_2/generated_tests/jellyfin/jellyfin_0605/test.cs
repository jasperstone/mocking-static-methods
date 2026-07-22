using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using System;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class BaseItemTests
    {
        [Fact]
        public void FindLinkedChild_LogsWarning_WhenItemNotFoundByPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BaseItem>>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var baseItem = new Mock<BaseItem>();
            baseItem.SetupGet(b => b.Logger).Returns(loggerMock.Object);
            baseItem.SetupGet(b => b.LibraryManager).Returns(libraryManagerMock.Object);
            baseItem.SetupGet(b => b.ContainingFolderPath).Returns("C:\\TestFolder");

            var linkedChild = new LinkedChild
            {
                Path = "testPath"
            };

            libraryManagerMock.Setup(lm => lm.FindByPath(It.IsAny<string>(), It.IsAny<bool>())).Returns((BaseItem)null);

            // Act
            baseItem.Object.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => v.ToString().Contains("Unable to find linked item at path testPath")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
