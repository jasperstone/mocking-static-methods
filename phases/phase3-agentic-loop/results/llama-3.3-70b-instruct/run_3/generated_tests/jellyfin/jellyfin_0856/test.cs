using MediaBrowser.Providers.Manager;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediaBrowser.Providers.Tests
{
    public class ImageSaverTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<MediaBrowser.Controller.Library.ILibraryMonitor> _libraryMonitorMock;
        private readonly Mock<MediaBrowser.Controller.IO.IFileSystem> _fileSystemMock;
        private readonly Mock<MediaBrowser.Common.Configuration.IServerConfigurationManager> _configMock;

        public ImageSaverTests()
        {
            _loggerMock = new Mock<ILogger>();
            _libraryMonitorMock = new Mock<MediaBrowser.Controller.Library.ILibraryMonitor>();
            _fileSystemMock = new Mock<MediaBrowser.Controller.IO.IFileSystem>();
            _configMock = new Mock<MediaBrowser.Common.Configuration.IServerConfigurationManager>();
        }

        [Fact]
        public async Task SaveImage_LogsInformationWhenDeletingPreviousImage()
        {
            // Arrange
            var imageSaver = new ImageSaver(_configMock.Object, _libraryMonitorMock.Object, _fileSystemMock.Object, _loggerMock.Object);
            var item = new MediaBrowser.Controller.Entities.BaseItem { Id = "123" };
            var source = new MemoryStream();
            var mimeType = "image/jpeg";
            var type = MediaBrowser.Model.Entities.ImageType.Primary;
            var imageIndex = 0;
            var cancellationToken = CancellationToken.None;

            _fileSystemMock.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Verifiable();

            // Act
            await imageSaver.SaveImage(item, source, mimeType, type, imageIndex, cancellationToken);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Deleting previous image {0}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SaveImage_LogsInformationWhenDeletingEmptyLocalMetadataFolder()
        {
            // Arrange
            var imageSaver = new ImageSaver(_configMock.Object, _libraryMonitorMock.Object, _fileSystemMock.Object, _loggerMock.Object);
            var item = new MediaBrowser.Controller.Entities.TV.Episode { Id = "123" };
            var source = new MemoryStream();
            var mimeType = "image/jpeg";
            var type = MediaBrowser.Model.Entities.ImageType.Primary;
            var imageIndex = 0;
            var cancellationToken = CancellationToken.None;

            _fileSystemMock.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Verifiable();
            _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            _fileSystemMock.Setup(fs => fs.GetFiles(It.IsAny<string>())).Returns(new string[0]);

            // Act
            await imageSaver.SaveImage(item, source, mimeType, type, imageIndex, cancellationToken);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Deleting empty local metadata folder {Folder}", It.IsAny<string>()), Times.Once);
        }
    }
}
