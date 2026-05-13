using Xunit;
using Moq;
using MediaBrowser.Providers.Manager;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using System.Threading;
using System;

namespace MediaBrowser.Providers.Manager.Tests
{
    public class ImageSaverTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<ILibraryMonitor> _libraryMonitorMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IServerConfigurationManager> _configMock;
        private readonly ImageSaver _imageSaver;

        public ImageSaverTests()
        {
            _loggerMock = new Mock<ILogger>();
            _libraryMonitorMock = new Mock<ILibraryMonitor>();
            _fileSystemMock = new Mock<IFileSystem>();
            _configMock = new Mock<IServerConfigurationManager>();
            _imageSaver = new ImageSaver(_configMock.Object, _libraryMonitorMock.Object, _fileSystemMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task SaveImage_DeletesPreviousImage_LogsInformation()
        {
            // Arrange
            var item = new Episode();
            var source = new MemoryStream();
            var mimeType = "image/jpeg";
            var type = ImageType.Primary;
            var imageIndex = 0;
            var cancellationToken = CancellationToken.None;

            var currentImagePath = "path/to/current/image.jpg";
            var parentDirectoryPath = "path/to/parent/directory";

            _fileSystemMock.Setup(fs => fs.DeleteFile(currentImagePath)).Verifiable();
            _fileSystemMock.Setup(fs => fs.DirectoryExists(parentDirectoryPath)).Returns(true);
            _fileSystemMock.Setup(fs => fs.GetFiles(parentDirectoryPath)).Returns(Array.Empty<string>());

            _configMock.Setup(c => c.ApplicationPaths.InternalMetadataPath).Returns("internal/metadata/path");

            // Act
            await _imageSaver.SaveImage(item, source, mimeType, type, imageIndex, cancellationToken);

            // Assert
            _loggerMock.Verify(logger => logger.LogInformation("Deleting previous image {0}", currentImagePath), Times.Once);
            _loggerMock.Verify(logger => logger.LogInformation("Deleting empty local metadata folder {Folder}", parentDirectoryPath), Times.Once);
        }
    }
}
