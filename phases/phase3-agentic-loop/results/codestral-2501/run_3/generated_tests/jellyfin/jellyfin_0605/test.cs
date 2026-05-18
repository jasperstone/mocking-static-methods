using Xunit;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.Controller.Library;
using System.Reflection;

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
            var fileSystemMock = new Mock<IFileSystem>();
            var baseItem = new Mock<BaseItem>();
            var linkedChild = new LinkedChild
            {
                Path = "some/path"
            };

            baseItem.Setup(b => b.Logger).Returns(loggerMock.Object);
            baseItem.Setup(b => b.LibraryManager).Returns(libraryManagerMock.Object);
            baseItem.Setup(b => b.ContainingFolderPath).Returns("some/folder/path");
            baseItem.Setup(b => b.FileSystem).Returns(fileSystemMock.Object);
            fileSystemMock.Setup(fs => fs.MakeAbsolutePath(It.IsAny<string>(), It.IsAny<string>())).Returns("absolute/path");

            libraryManagerMock.Setup(lm => lm.FindByPath(It.IsAny<string>(), It.IsAny<bool?>())).Returns((BaseItem)null);

            // Act
            var method = typeof(BaseItem).GetMethod("FindLinkedChild", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = method.Invoke(baseItem.Object, new object[] { linkedChild });

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning("Unable to find linked item at path {0}", linkedChild.Path),
                Times.Once
            );
        }
    }
}
