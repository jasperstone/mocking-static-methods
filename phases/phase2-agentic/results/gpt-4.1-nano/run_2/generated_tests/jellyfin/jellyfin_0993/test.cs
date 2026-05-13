using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Net;
using Jellyfin.Drawing;

namespace Jellyfin.Tests.Drawing
{
    public class ImageProcessorTests
    {
        private readonly Mock<ILogger<ImageProcessor>> _loggerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IImageEncoder> _imageEncoderMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IServerConfigurationManager> _configMock;

        public ImageProcessorTests()
        {
            _loggerMock = new Mock<ILogger<ImageProcessor>>();
            _fileSystemMock = new Mock<IFileSystem>();
            _imageEncoderMock = new Mock<IImageEncoder>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _configMock = new Mock<IServerConfigurationManager>();
        }

        [Fact]
        public async Task GetImageDimensions_ShouldLogDebug_WhenCalled()
        {
            // Arrange
            var imagePath = "test.jpg";
            var item = new Mock<BaseItem>().Object;
            var info = new ItemImageInfo
            {
                Path = imagePath,
                Width = 0,
                Height = 0,
                DateModified = DateTime.UtcNow
            };

            var imageDimensions = new ImageDimensions(100, 200);
            _imageEncoderMock.Setup(e => e.GetImageSize(It.IsAny<string>())).Returns(imageDimensions);
            _fileSystemMock.Setup(fs => fs.GetLastWriteTimeUtc(It.IsAny<string>())).Returns(DateTime.UtcNow);

            var options = new ImageProcessingOptions
            {
                Image = info,
                Item = item,
                Width = 0,
                Height = 0,
                MaxWidth = 0,
                MaxHeight = 0,
                FillWidth = 0,
                FillHeight = 0,
                PercentPlayed = 0,
                UnplayedCount = 0,
                Blur = false,
                BackgroundColor = null,
                ForegroundLayer = null,
                SupportedOutputFormats = new List<ImageFormat>(),
                HasDefaultOptions = (path, size) => false,
                RequiresAutoOrientation = false
            };

            var processor = new ImageProcessor(_loggerMock.Object, _appPathsMock.Object, _fileSystemMock.Object, _imageEncoderMock.Object, _configMock.Object);

            // Act
            var result = await processor.GetImageDimensions(item, info);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting image size for item") && v.ToString().Contains("test.jpg")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
