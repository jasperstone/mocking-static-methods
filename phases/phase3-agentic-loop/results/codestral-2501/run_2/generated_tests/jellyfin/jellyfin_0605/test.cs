using Xunit;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.Controller.Library;
using System.Threading;

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
            var baseItem = new Mock<DerivedBaseItem>();
            var linkedChild = new LinkedChild
            {
                Path = "some/path"
            };

            baseItem.Setup(b => b.Logger).Returns(loggerMock.Object);
            baseItem.Setup(b => b.LibraryManager).Returns(libraryManagerMock.Object);
            baseItem.Setup(b => b.ContainingFolderPath).Returns("some/folder/path");
            baseItem.Setup(b => b.FileSystem.MakeAbsolutePath(It.IsAny<string>(), It.IsAny<string>())).Returns("absolute/path");

            libraryManagerMock.Setup(lm => lm.FindByPath(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns((BaseItem)null);

            // Act
            var result = baseItem.Object.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning("Unable to find linked item at path {0}", linkedChild.Path),
                Times.Once);
        }

        private class DerivedBaseItem : BaseItem
        {
            public new BaseItem FindLinkedChild(LinkedChild info)
            {
                return base.FindLinkedChild(info);
            }
        }
    }
}
