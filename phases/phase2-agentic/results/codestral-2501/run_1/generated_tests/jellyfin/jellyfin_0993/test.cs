using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Controller.Configuration;
using Jellyfin.Drawing;

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        private readonly Mock<ILogger<ImageProcessor>> _loggerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IImageEncoder> _imageEncoderMock;
        private readonly Mock<IServerConfigurationManager> _configMock;
        private readonly ImageProcessor _imageProcessor;

        public ImageProcessorTests()
        {
            _loggerMock = new Mock<ILogger<ImageProcessor>>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _imageEncoderMock = new Mock<IImageEncoder>();
            _configMock = new Mock<IServerConfigurationManager>();

            _configMock.Setup(c => c.Configuration).Returns(new ServerConfiguration { ParallelImageEncodingLimit = 4 });

            _imageProcessor = new ImageProcessor(
                _loggerMock.Object,
                _appPathsMock.Object,
                _fileSystemMock.Object,
                _imageEncoderMock.Object,
                _configMock.Object);
        }

        [Fact]
        public void GetImageDimensions_LogsDebug_WhenHeightAndWidthAreZero()
        {
            // Arrange
            var item = new BaseItem();
            var info = new ItemImageInfo { Width = 0, Height = 0, Path = "testPath" };
            var expectedDimensions = new ImageDimensions(100, 200);

            _imageEncoderMock.Setup(e => e.GetImageSize(info.Path)).Returns(expectedDimensions);

            // Act
            var result = _imageProcessor.GetImageDimensions(item, info);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug(
                    "Getting image size for item {ItemType} {Path}",
                    It.IsAny<object[]>()),
                Times.Once);

            Assert.Equal(expectedDimensions, result);
        }

        [Fact]
        public void GetImageDimensions_ReturnsDimensions_WhenHeightAndWidthAreNonZero()
        {
            // Arrange
            var item = new BaseItem();
            var info = new ItemImageInfo { Width = 100, Height = 200 };

            // Act
            var result = _imageProcessor.GetImageDimensions(item, info);

            // Assert
            Assert.Equal(new ImageDimensions(100, 200), result);
        }

        [Fact]
        public void GetImageDimensions_ReturnsDimensionsFromPath()
        {
            // Arrange
            var path = "testPath";
            var expectedDimensions = new ImageDimensions(100, 200);

            _imageEncoderMock.Setup(e => e.GetImageSize(path)).Returns(expectedDimensions);

            // Act
            var result = _imageProcessor.GetImageDimensions(path);

            // Assert
            Assert.Equal(expectedDimensions, result);
        }

        [Fact]
        public void GetImageBlurHash_ReturnsEmpty_WhenDimensionsAreInvalid()
        {
            // Arrange
            var path = "testPath";
            var invalidDimensions = new ImageDimensions(0, 0);

            _imageEncoderMock.Setup(e => e.GetImageSize(path)).Returns(invalidDimensions);

            // Act
            var result = _imageProcessor.GetImageBlurHash(path);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetImageBlurHash_ReturnsBlurHash_WhenDimensionsAreValid()
        {
            // Arrange
            var path = "testPath";
            var validDimensions = new ImageDimensions(100, 200);
            var expectedBlurHash = "blurHash";

            _imageEncoderMock.Setup(e => e.GetImageSize(path)).Returns(validDimensions);
            _imageEncoderMock.Setup(e => e.GetImageBlurHash(path, validDimensions)).Returns(expectedBlurHash);

            // Act
            var result = _imageProcessor.GetImageBlurHash(path);

            // Assert
            Assert.Equal(expectedBlurHash, result);
        }
    }
}
