using System;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        private readonly Mock<ILogger<ImageProcessor>> _loggerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IImageEncoder> _imageEncoderMock;
        private readonly Mock<IServerConfigurationManager> _configMock;

        public ImageProcessorTests()
        {
            _loggerMock = new Mock<ILogger<ImageProcessor>>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _imageEncoderMock = new Mock<IImageEncoder>();
            _configMock = new Mock<IServerConfigurationManager>();
        }

        [Fact]
        public void GetImageDimensions_WhenWidthAndHeightAreZero_LogsDebugMessage()
        {
            // Arrange
            var item = new Mock<BaseItem>().Object;
            var info = new ItemImageInfo
            {
                Path = "/test/image.jpg",
                Width = 0,
                Height = 0
            };

            _imageEncoderMock
                .Setup(e => e.GetImageSize("/test/image.jpg"))
                .Returns(new ImageDimensions(1920, 1080));

            var imageProcessor = CreateImageProcessor();

            // Act
            var result = imageProcessor.GetImageDimensions(item, info);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogDebug(
                    It.Is<string>(msg => msg.Contains("Getting image size for item {ItemType} {Path}")),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );

            Assert.Equal(1920, result.Width);
            Assert.Equal(1080, result.Height);
            Assert.Equal(1920, info.Width);
            Assert.Equal(1080, info.Height);
        }

        [Fact]
        public void GetImageDimensions_WhenWidthAndHeightArePositive_DoesNotLogDebugMessage()
        {
            // Arrange
            var item = new Mock<BaseItem>().Object;
            var info = new ItemImageInfo
            {
                Path = "/test/image.jpg",
                Width = 1920,
                Height = 1080
            };

            var imageProcessor = CreateImageProcessor();

            // Act
            var result = imageProcessor.GetImageDimensions(item, info);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogDebug(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()
                ),
                Times.Never
            );

            Assert.Equal(1920, result.Width);
            Assert.Equal(1080, result.Height);
        }

        private ImageProcessor CreateImageProcessor()
        {
            return new ImageProcessor(
                _loggerMock.Object,
                _appPathsMock.Object,
                _fileSystemMock.Object,
                _imageEncoderMock.Object,
                _configMock.Object);
        }
    }
}
