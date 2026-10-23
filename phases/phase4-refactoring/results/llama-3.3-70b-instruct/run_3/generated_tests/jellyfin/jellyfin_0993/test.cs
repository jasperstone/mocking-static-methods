using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Drawing;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        private readonly Mock<ILogger<ImageProcessor>> _loggerMock;
        private readonly Mock<MediaBrowser.Common.IO.IFileSystem> _fileSystemMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IImageEncoder> _imageEncoderMock;

        public ImageProcessorTests()
        {
            _loggerMock = new Mock<ILogger<ImageProcessor>>();
            _fileSystemMock = new Mock<MediaBrowser.Common.IO.IFileSystem>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _imageEncoderMock = new Mock<IImageEncoder>();
        }

        [Fact]
        public async Task GetImageDimensions_LogsDebugMessage()
        {
            // Arrange
            var item = new BaseItem { };
            var info = new ItemImageInfo { Path = "path" };
            var imageProcessor = new ImageProcessor(_loggerMock.Object, _appPathsMock.Object, _fileSystemMock.Object, _imageEncoderMock.Object, null);

            // Act
            imageProcessor.GetImageDimensions(item, info);

            // Assert
            _loggerMock.Verify(logger => logger.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
