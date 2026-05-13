using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
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
            var libraryMonitorMock = new Mock<ILibraryMonitor>();
            var fileSystemMock = new Mock<IFileSystem>();
            var configMock = new Mock<IServerConfigurationManager>();
            var item = new Mock<BaseItem>();

            var imageSaver = new ImageSaver(configMock.Object, libraryMonitorMock.Object, fileSystemMock.Object, loggerMock.Object);

            // Act
            await imageSaver.SaveImage(item.Object, new MemoryStream(), "image/jpeg", ImageType.Primary, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Deleting previous image {0}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SaveImage_LogsDeletingEmptyLocalMetadataFolder()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var libraryMonitorMock = new Mock<ILibraryMonitor>();
            var fileSystemMock = new Mock<IFileSystem>();
            var configMock = new Mock<IServerConfigurationManager>();
            var item = new Mock<BaseItem>();

            var imageSaver = new ImageSaver(configMock.Object, libraryMonitorMock.Object, fileSystemMock.Object, loggerMock.Object);

            // Act
            await imageSaver.SaveImage(item.Object, new MemoryStream(), "image/jpeg", ImageType.Primary, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Deleting empty local metadata folder {Folder}", It.IsAny<string>()), Times.Once);
        }
    }
}
