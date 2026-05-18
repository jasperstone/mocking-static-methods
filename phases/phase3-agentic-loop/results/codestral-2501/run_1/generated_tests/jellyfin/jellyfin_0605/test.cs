using Xunit;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.IO;
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
            var fileSystemMock = new Mock<IFileSystem>();
            var linkedChild = new LinkedChild
            {
                Path = "some/path"
            };

            var baseItem = new Mock<BaseItemSubclass>
            {
                CallBase = true
            };

            baseItem.Setup(b => b.Logger).Returns(loggerMock.Object);
            baseItem.Setup(b => b.LibraryManager).Returns(libraryManagerMock.Object);
            baseItem.Setup(b => b.ContainingFolderPath).Returns("some/folder/path");
            baseItem.Setup(b => b.FileSystem).Returns(fileSystemMock.Object);

            fileSystemMock.Setup(fs => fs.MakeAbsolutePath(It.IsAny<string>(), It.IsAny<string>())).Returns("some/absolute/path");
            libraryManagerMock.Setup(lm => lm.FindByPath(It.IsAny<string>(), It.IsAny<bool>())).Returns((BaseItem)null);

            // Act
            var result = baseItem.Object.FindLinkedChild(linkedChild);

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning("Unable to find linked item at path {0}", linkedChild.Path),
                Times.Once
            );
        }

        private class BaseItemSubclass : BaseItem
        {
            public new ILogger<BaseItem> Logger { get; set; }
            public new ILibraryManager LibraryManager { get; set; }
            public new IFileSystem FileSystem { get; set; }
            public new string ContainingFolderPath { get; set; }
        }
    }
}
