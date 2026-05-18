using System;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Model.Drawing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        [Fact]
        public void GetImageDimensions_WithZeroWidthAndHeight_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ImageProcessor>>();
            var imageEncoderMock = new Mock<IImageEncoder>();
            imageEncoderMock.Setup(e => e.GetImageSize(It.IsAny<string>()))
                           .Returns(new ImageDimensions(1920, 1080));

            var fileSystemMock = new Mock<IFileSystem>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var configMock = new Mock<IServerConfigurationManager>();

            var processor = new ImageProcessor(
                loggerMock.Object,
                appPathsMock.Object,
                fileSystemMock.Object,
                imageEncoderMock.Object,
                configMock.Object);

            var item = new Mock<BaseItem>().Object;
            var info = new ItemImageInfo
            {
                Path = "/test/image.jpg",
                Width = 0,
                Height = 0
            };

            // Act
            processor.GetImageDimensions(item, info);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    "Getting image size for item {ItemType} {Path}",
                    It.IsAny<object>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void GetImageDimensions_WithValidDimensions_DoesNotLogDebug()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ImageProcessor>>();
            var imageEncoderMock = new Mock<IImageEncoder>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var configMock = new Mock<IServerConfigurationManager>();

            var processor = new ImageProcessor(
                loggerMock.Object,
                appPathsMock.Object,
                fileSystemMock.Object,
                imageEncoderMock.Object,
                configMock.Object);

            var item = new Mock<BaseItem>().Object;
            var info = new ItemImageInfo
            {
                Path = "/test/image.jpg",
                Width = 1920,
                Height = 1080
            };

            // Act
            processor.GetImageDimensions(item, info);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }
    }
}
