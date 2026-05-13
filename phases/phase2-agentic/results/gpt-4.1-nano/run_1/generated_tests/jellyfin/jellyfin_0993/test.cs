using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.IO;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Configuration;
using Jellyfin.Drawing;

namespace Jellyfin.Tests
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
        public async Task GetImageDimensions_CallsLogDebug_WhenHeightOrWidthIsZero()
        {
            // Arrange
            var imageProcessor = new ImageProcessor(
                _loggerMock.Object,
                _appPathsMock.Object,
                _fileSystemMock.Object,
                _imageEncoderMock.Object,
                _configMock.Object);

            var item = new Mock<BaseItem>().Object;
            var info = new ItemImageInfo
            {
                Path = "test.jpg",
                Width = 0,
                Height = 0,
                DateModified = DateTime.UtcNow
            };

            _imageEncoderMock.Setup(e => e.GetImageSize(It.IsAny<string>()))
                .Returns(new ImageDimensions(100, 200));

            // Act
            var result = await imageProcessor.GetImageDimensions(item, info);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting image size for item")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
