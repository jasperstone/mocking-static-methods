using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediaBrowser.Providers.Manager
{
    public class ImageSaverTests
    {
        [Fact]
        public async Task SaveImage_LogsDeletingPreviousImage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryMonitorMock = new Mock<MediaBrowser.Controller.Library.ILibraryMonitor>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var configMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var imageSaver = new ImageSaver(configMock.Object, libraryMonitorMock.Object, fileSystemMock.Object, loggerMock.Object);

            var item = new MediaBrowser.Controller.Entities.Movie { Id = Guid.NewGuid() };
            var source = new MemoryStream();
            var mimeType = "image/jpeg";
            var type = MediaBrowser.Model.Entities.ImageType.Primary;
            var imageIndex = 0;
            var saveLocally = true;

            // Act
            await imageSaver.SaveImage(item, source, mimeType, type, imageIndex, saveLocally, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Deleting previous image {0}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SaveImage_LogsDeletingEmptyLocalMetadataFolder()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryMonitorMock = new Mock<MediaBrowser.Controller.Library.ILibraryMonitor>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var configMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var imageSaver = new ImageSaver(configMock.Object, libraryMonitorMock.Object, fileSystemMock.Object, loggerMock.Object);

            var item = new Episode { Id = Guid.NewGuid() };
            var source = new MemoryStream();
            var mimeType = "image/jpeg";
            var type = MediaBrowser.Model.Entities.ImageType.Primary;
            var imageIndex = 0;
            var saveLocally = true;

            // Act
            await imageSaver.SaveImage(item, source, mimeType, type, imageIndex, saveLocally, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Deleting empty local metadata folder {Folder}", It.IsAny<string>()), Times.Once);
        }
    }
}
