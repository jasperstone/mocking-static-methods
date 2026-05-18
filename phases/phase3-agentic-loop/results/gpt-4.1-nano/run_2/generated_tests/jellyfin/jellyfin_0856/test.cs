using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Providers.Manager;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Tests.Providers.Manager
{
    public class ImageSaverTests
    {
        private readonly Mock<IServerConfigurationManager> _configMock;
        private readonly Mock<ILibraryMonitor> _libraryMonitorMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<ILogger> _loggerMock;

        public ImageSaverTests()
        {
            _configMock = new Mock<IServerConfigurationManager>();
            _libraryMonitorMock = new Mock<ILibraryMonitor>();
            _fileSystemMock = new Mock<IFileSystem>();
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public async Task SaveImage_Should_LogInformation_When_DeletingPreviousImage()
        {
            // Arrange
            var imageSaver = new TestImageSaver(_configMock.Object, _libraryMonitorMock.Object, _fileSystemMock.Object, _loggerMock.Object);

            var item = new Mock<BaseItem>();
            item.Setup(i => i.SupportsLocalMetadata).Returns(true);
            item.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            item.Setup(i => i.ExtraType).Returns((ImageType?)null);
            item.Setup(i => i.IsFileProtocol).Returns(true);
            item.Setup(i => i.AllowsMultipleImages(It.IsAny<ImageType>())).Returns(false);
            item.Setup(i => i.GetImages(It.IsAny<ImageType>())).Returns(new[] { new Image { Path = "oldpath.jpg", IsLocalFile = true } });
            item.Setup(i => i is Episode).Returns(true);
            item.Setup(i => i.Path).Returns("oldpath.jpg");

            var currentImage = new Image { Path = "oldpath.jpg", IsLocalFile = true };
            var testInstance = new TestImageSaver(_configMock.Object, _libraryMonitorMock.Object, _fileSystemMock.Object, _loggerMock.Object, currentImage);

            _fileSystemMock.Setup(fs => fs.DeleteFile(It.IsAny<string>()));
            _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Returns(true);
            _fileSystemMock.Setup(fs => fs.GetFiles(It.IsAny<string>())).Returns(Array.Empty<string>());

            // Act
            await testInstance.SaveImage(item.Object, new MemoryStream(new byte[] { 1, 2, 3 }), "image/jpeg", ImageType.Primary, 0, false, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting previous image")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper class to override GetCurrentImage
        private class TestImageSaver : ImageSaver
        {
            private readonly Image _currentImage;

            public TestImageSaver(IServerConfigurationManager config, ILibraryMonitor libraryMonitor, IFileSystem fileSystem, ILogger logger, Image currentImage = null)
                : base(config, libraryMonitor, fileSystem, logger)
            {
                _currentImage = currentImage;
            }

            protected override Image GetCurrentImage(BaseItem item, ImageType type, int index)
            {
                return _currentImage;
            }
        }
    }
}
