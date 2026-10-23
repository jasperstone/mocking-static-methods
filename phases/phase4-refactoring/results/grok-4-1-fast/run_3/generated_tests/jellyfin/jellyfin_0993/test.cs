using System;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.IO;

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        private readonly Mock<ILogger<ImageProcessor>> _loggerMock;
        private readonly Mock<IImageEncoder> _imageEncoderMock;
        private readonly Mock<IServerConfigurationManager> _configMock;

        public ImageProcessorTests()
        {
            _loggerMock = new Mock<ILogger<ImageProcessor>>();
            _imageEncoderMock = new Mock<IImageEncoder>();
            _configMock = new Mock<IServerConfigurationManager>();
        }

        [Fact]
        public void GetImageDimensions_WithZeroWidthAndHeight_LogsDebugMessage()
        {
            // Arrange
            var item = new Mock<BaseItem>().Object;
            var info = new ItemImageInfo
            {
                Path = "/test/image.jpg",
                Width = 0,
                Height = 0
            };

            _imageEncoderMock.Setup(x => x.GetImageSize(It.IsAny<string>()))
                .Returns(new ImageDimensions(100, 100));

            SetupConfig();
            var imageProcessor = CreateImageProcessor();

            // Act
            imageProcessor.GetImageDimensions(item, info);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug("Getting image size for item {ItemType} {Path}", item.GetType().Name, info.Path),
                Times.Once);
        }

        [Fact]
        public void GetImageDimensions_WithValidDimensions_DoesNotLogDebugMessage()
        {
            // Arrange
            var item = new Mock<BaseItem>().Object;
            var info = new ItemImageInfo
            {
                Path = "/test/image.jpg",
                Width = 1920,
                Height = 1080
            };

            SetupConfig();
            var imageProcessor = CreateImageProcessor();

            // Act
            imageProcessor.GetImageDimensions(item, info);

            // Assert
            _loggerMock.Verify(
                x => x.LogDebug("Getting image size for item {ItemType} {Path}", It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        private void SetupConfig()
        {
            _configMock.Setup(x => x.Configuration)
                .Returns(new ServerConfiguration
                {
                    ParallelImageEncodingLimit = 1
                });
        }

        private ImageProcessor CreateImageProcessor()
        {
            return new ImageProcessor(
                _loggerMock.Object,
                new Mock<IServerApplicationPaths>().Object,
                new Mock<IFileSystem>().Object,
                _imageEncoderMock.Object,
                _configMock.Object);
        }
    }
}
