using Xunit;
using Moq;
using System.IO;
using System.Threading;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Providers.Manager.Tests
{
    public class ImageSaverTests
    {
        [Fact]
        public void SaveImage_LogsDeletingPreviousImage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryMonitorMock = new Mock<ILibraryMonitor>();
            var fileSystemMock = new Mock<IFileSystem>();
            var configMock = new Mock<IServerConfigurationManager>();
            var item = new Mock<BaseItem>();
            var source = new MemoryStream();
            var mimeType = "image/jpeg";
            var type = ImageType.Primary;
            var imageIndex = 0;
            var saveLocally = true;
            var cancellationToken = CancellationToken.None;

            var imageSaver = new ImageSaver(configMock.Object, libraryMonitorMock.Object, fileSystemMock.Object, loggerMock.Object);

            // Act
            imageSaver.SaveImage(item.Object, source, mimeType, type, imageIndex, saveLocally, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Deleting previous image {0}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void SaveImage_LogsDeletingEmptyLocalMetadataFolder()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryMonitorMock = new Mock<ILibraryMonitor>();
            var fileSystemMock = new Mock<IFileSystem>();
            var configMock = new Mock<IServerConfigurationManager>();
            var item = new Mock<BaseItem>();
            var source = new MemoryStream();
            var mimeType = "image/jpeg";
            var type = ImageType.Primary;
            var imageIndex = 0;
            var saveLocally = true;
            var cancellationToken = CancellationToken.None;

            var imageSaver = new ImageSaver(configMock.Object, libraryMonitorMock.Object, fileSystemMock.Object, loggerMock.Object);

            // Act
            imageSaver.SaveImage(item.Object, source, mimeType, type, imageIndex, saveLocally, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Deleting empty local metadata folder {Folder}", It.IsAny<string>()), Times.Once);
        }
    }
}
