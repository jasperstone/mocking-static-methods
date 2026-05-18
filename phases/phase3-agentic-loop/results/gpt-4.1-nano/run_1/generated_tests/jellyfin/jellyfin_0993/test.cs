using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IImageEncoder> _imageEncoderMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IServerConfigurationManager> _configMock;

        private readonly ImageProcessor _imageProcessor;

        public ImageProcessorTests()
        {
            _loggerMock = new Mock<ILogger<ImageProcessor>>();
            _fileSystemMock = new Mock<IFileSystem>();
            _imageEncoderMock = new Mock<IImageEncoder>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _configMock = new Mock<IServerConfigurationManager>();

            _appPathsMock.SetupGet(p => p.ImageCachePath).Returns("cache");
            _configMock.SetupGet(c => c.Configuration.ParallelImageEncodingLimit).Returns(1);

            _imageEncoderMock.SetupGet(e => e.SupportsImageEncoding).Returns(true);
            _imageEncoderMock.SetupGet(e => e.SupportedOutputFormats).Returns(new List<ImageFormat> { new ImageFormat("jpeg", "image/jpeg") });
            _imageEncoderMock.Setup(e => e.EncodeImage(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<ImageOrientation?>(), It.IsAny<int>(), It.IsAny<ImageProcessingOptions>(), It.IsAny<ImageFormat>()))
                .Returns<string, DateTime, string, bool, ImageOrientation?, int, ImageProcessingOptions, ImageFormat>((origPath, date, cachePath, autoOrient, orientation, quality, options, format) => cachePath);

            _imageProcessor = new ImageProcessor(_loggerMock.Object, _appPathsMock.Object, _fileSystemMock.Object, _imageEncoderMock.Object, _configMock.Object);
        }

        [Fact]
        public async Task GetImageDimensions_LogsDebug_WhenHeightOrWidthIsZero()
        {
            // Arrange
            var options = new ImageProcessingOptions
            {
                Image = new ItemImageInfo { Path = "path.jpg", Width = 0, Height = 0, DateModified = DateTime.Now },
                Item = new BaseItem()
            };

            var mockLogger = new Mock<ILogger>();
            var imageProcessor = new ImageProcessor(mockLogger.Object, _appPathsMock.Object, _fileSystemMock.Object, _imageEncoderMock.Object, _configMock.Object);

            // Act
            await imageProcessor.GetImageDimensions(options);

            // Assert
            mockLogger.VerifyLogDebug("Getting image size for item {ItemType} {Path}", Times.Once());
        }

        [Fact]
        public async Task ProcessImage_ReturnsOriginal_WhenFileDoesNotExist()
        {
            // Arrange
            var options = new ImageProcessingOptions
            {
                Image = new ItemImageInfo { Path = "nonexistent.jpg", Width = 0, Height = 0, DateModified = DateTime.Now },
                Item = new BaseItem()
            };

            _fileSystemMock.Setup(fs => fs.GetLastWriteTimeUtc(It.IsAny<string>())).Returns(DateTime.Now);
            _fileSystemMock.Setup(fs => fs.Exists(It.IsAny<string>())).Returns(false);

            // Act
            var result = await _imageProcessor.ProcessImage(options);

            // Assert
            Assert.Equal(options.Image.Path, result.Path);
        }

        [Fact]
        public async Task ProcessImage_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var options = new ImageProcessingOptions
            {
                Image = new ItemImageInfo { Path = "path.jpg", Width = 0, Height = 0, DateModified = DateTime.Now },
                Item = new BaseItem()
            };

            _imageEncoderMock.Setup(e => e.EncodeImage(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<ImageOrientation?>(), It.IsAny<int>(), It.IsAny<ImageProcessingOptions>(), It.IsAny<ImageFormat>()))
                .Throws(new Exception("Encoding failed"));

            // Act
            var result = await _imageProcessor.ProcessImage(options);

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error encoding image"), Times.Once);
            Assert.Equal(options.Image.Path, result.Path);
        }
    }
}
