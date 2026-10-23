using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;

namespace MediaBrowser.Providers.Manager
{
    public class ImageSaverTests
    {
        [Fact]
        public async Task SaveImage_LogsInformation_WhenDeletingPreviousImage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryMonitorMock = new Mock<ILibraryMonitor>();
            var fileSystemMock = new Mock<IFileSystem>();
            var configMock = new Mock<IServerConfigurationManager>();
            var imageSaver = new ImageSaver(configMock.Object, libraryMonitorMock.Object, fileSystemMock.Object, loggerMock.Object);

            var item = new MediaBrowser.Controller.Entities.Folder { Id = Guid.NewGuid() };
            var source = new MemoryStream();
            var mimeType = "image/jpeg";
            var type = MediaBrowser.Model.Entities.ImageType.Primary;
            var imageIndex = 0;
            var cancellationToken = CancellationToken.None;

            // Act
            await imageSaver.SaveImage(item, source, mimeType, type, imageIndex, cancellationToken);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Deleting previous image {0}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SaveImage_LogsInformation_WhenDeletingEmptyLocalMetadataFolder()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryMonitorMock = new Mock<ILibraryMonitor>();
            var fileSystemMock = new Mock<IFileSystem>();
            var configMock = new Mock<IServerConfigurationManager>();
            var imageSaver = new ImageSaver(configMock.Object, libraryMonitorMock.Object, fileSystemMock.Object, loggerMock.Object);

            var item = new MediaBrowser.Controller.Entities.Episode { Id = Guid.NewGuid() };
            var source = new MemoryStream();
            var mimeType = "image/jpeg";
            var type = MediaBrowser.Model.Entities.ImageType.Primary;
            var imageIndex = 0;
            var cancellationToken = CancellationToken.None;

            // Act
            await imageSaver.SaveImage(item, source, mimeType, type, imageIndex, cancellationToken);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Deleting empty local metadata folder {Folder}", It.IsAny<string>()), Times.Once);
        }
    }
}
