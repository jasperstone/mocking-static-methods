using Xunit;
using Moq;
using MediaBrowser.Providers.Manager;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace MediaBrowser.Providers.Manager.Tests
{
    public class ImageSaverTests
    {
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<ILibraryMonitor> _libraryMonitorMock;
        private readonly Mock<ILogger<ImageSaver>> _loggerMock;
        private readonly Mock<IServerConfigurationManager> _configMock;
        private readonly ImageSaver _imageSaver;

        public ImageSaverTests()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _libraryMonitorMock = new Mock<ILibraryMonitor>();
            _loggerMock = new Mock<ILogger<ImageSaver>>();
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
            var newImagePath = "path/to/new/image.jpg";

            _fileSystemMock.Setup(fs => fs.DeleteFile(currentImagePath)).Verifiable();
            _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            _fileSystemMock.Setup(fs => fs.GetFiles(It.IsAny<string>())).Returns(Array.Empty<string>());

            _configMock.Setup(c => c.ApplicationPaths.InternalMetadataPath).Returns("internal/metadata/path");

            // Act
            await _imageSaver.SaveImage(item, source, mimeType, type, imageIndex, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting previous image")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting empty local metadata folder")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            _fileSystemMock.Verify();
        }
    }
}
