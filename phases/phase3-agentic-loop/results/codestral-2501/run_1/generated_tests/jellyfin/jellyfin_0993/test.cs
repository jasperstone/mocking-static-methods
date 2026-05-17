using System;
using System.Threading.Tasks;
using AsyncKeyedLock;
using Jellyfin.Drawing;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
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
        public void GetImageDimensions_ShouldLogDebug_WhenHeightAndWidthAreZero()
        {
            // Arrange
            var item = new Mock<BaseItem>().Object;
            var info = new ItemImageInfo { Path = "testPath" };
            var imageProcessor = new ImageProcessor(_loggerMock.Object, _appPathsMock.Object, _fileSystemMock.Object, _imageEncoderMock.Object, _configMock.Object);

            _imageEncoderMock.Setup(encoder => encoder.GetImageSize(It.IsAny<string>())).Returns(new ImageDimensions(100, 200));

            // Act
            var result = imageProcessor.GetImageDimensions(item, info);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            Assert.Equal(100, result.Width);
            Assert.Equal(200, result.Height);
        }

        [Fact]
        public void GetImageDimensions_ShouldReturnDimensions_WhenHeightAndWidthAreNonZero()
        {
            // Arrange
            var item = new Mock<BaseItem>().Object;
            var info = new ItemImageInfo { Width = 100, Height = 200 };
            var imageProcessor = new ImageProcessor(_loggerMock.Object, _appPathsMock.Object, _fileSystemMock.Object, _imageEncoderMock.Object, _configMock.Object);

            // Act
            var result = imageProcessor.GetImageDimensions(item, info);

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
            Assert.Equal(100, result.Width);
            Assert.Equal(200, result.Height);
        }
    }
}
