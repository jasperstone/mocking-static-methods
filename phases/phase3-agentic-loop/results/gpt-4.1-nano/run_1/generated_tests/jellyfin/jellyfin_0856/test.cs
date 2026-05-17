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
        private readonly ImageSaver _imageSaver;

        public ImageSaverTests()
        {
            _configMock = new Mock<IServerConfigurationManager>();
            _libraryMonitorMock = new Mock<ILibraryMonitor>();
            _fileSystemMock = new Mock<IFileSystem>();
            _loggerMock = new Mock<ILogger>();

            _imageSaver = new ImageSaver(
                _configMock.Object,
                _libraryMonitorMock.Object,
                _fileSystemMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task SaveImage_Should_LogInformation_When_DeletingPreviousImage()
        {
            // Arrange
            var item = new Mock<BaseItem>();
            item.Setup(i => i.SupportsLocalMetadata).Returns(true);
            item.Setup(i => i.IsSaveLocalMetadataEnabled()).Returns(true);
            item.Setup(i => i.ExtraType).Returns((ImageType?)null);
            item.Setup(i => i.IsFileProtocol).Returns(true);
            item.Setup(i => i.AllowsMultipleImages(It.IsAny<ImageType>())).Returns(true);
            item.Setup(i => i.GetImages(It.IsAny<ImageType>())).Returns(new[] { new Image() });
            item.Setup(i => i is Episode).Returns(true);
            item.Setup(i => i.Path).Returns("oldpath");
            var stream = new MemoryStream();

            // Setup GetCurrentImage to return a local image with path "oldpath"
            var currentImage = new Image { Path = "oldpath", IsLocalFile = true };
            // We need to mock GetCurrentImage method to return currentImage
            // Since it's a private method, we will invoke SaveImage and verify logs

            // Act
            await _imageSaver.SaveImage(item.Object, stream, "image/jpeg", ImageType.Primary, 0, true, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting previous image")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
