using System;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        private readonly Mock<ILogger<ImageProcessor>> _loggerMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IImageEncoder> _imageEncoderMock;
        private readonly Mock<IServerConfigurationManager> _configMock;

        public ImageProcessorTests()
        {
            _loggerMock = new Mock<ILogger<ImageProcessor>>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _imageEncoderMock = new Mock<IImageEncoder>();
            _configMock = new Mock<IServerConfigurationManager>();
        }

        [Fact]
        public void GetImageDimensions_ItemHasNoDimensions_LogsDebugMessage()
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

            var imageProcessor = new ImageProcessor(
                _loggerMock.Object,
                _appPathsMock.Object,
                Mock.Of<Jellyfin.Model.IO.IFileSystem>(),
                _imageEncoderMock.Object,
                _configMock.Object);

            // Act
            imageProcessor.GetImageDimensions(item, info);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogDebug(
                    "Getting image size for item {ItemType} {Path}",
                    It.Is<string>(name => name == "BaseItem"),
                    It.Is<string>(path => path == "/test/image.jpg")),
                Times.Once);
        }

        [Fact]
        public void GetImageDimensions_ItemHasDimensions_DoesNotLogDebugMessage()
        {
            // Arrange
            var item = new Mock<BaseItem>().Object;
            var info = new ItemImageInfo
            {
                Path = "/test/image.jpg",
                Width = 1920,
                Height = 1080
            };

            var imageProcessor = new ImageProcessor(
                _loggerMock.Object,
                _appPathsMock.Object,
                Mock.Of<Jellyfin.Model.IO.IFileSystem>(),
                _imageEncoderMock.Object,
                _configMock.Object);

            // Act
            imageProcessor.GetImageDimensions(item, info);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogDebug(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    Times.Never());
        }
    }
}
