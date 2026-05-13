using System;
using System.Threading.Tasks;
using Jellyfin.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        [Fact]
        public void GetImageDimensions_WithValidWidthHeight_DoesNotLogDebug()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ImageProcessor>>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var appPathsMock = new Mock<MediaBrowser.Controller.IServerApplicationPaths>();
            var imageEncoderMock = new Mock<IImageEncoder>();
            var configMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            configMock.Setup(c => c.Configuration).Returns(new MediaBrowser.Controller.Configuration.ServerConfiguration { ParallelImageEncodingLimit = 1 });

            var processor = new ImageProcessor(loggerMock.Object, appPathsMock.Object, fileSystemMock.Object, imageEncoderMock.Object, configMock.Object);

            var item = new BaseItem();
            var info = new ItemImageInfo
            {
                Width = 100,
                Height = 200,
                Path = "somepath"
            };

            // Act
            var result = processor.GetImageDimensions(item, info);

            // Assert
            Assert.Equal(100, result.Width);
            Assert.Equal(200, result.Height);
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void GetImageDimensions_WithZeroWidthOrHeight_LogsDebugAndReturnsSizeFromEncoder()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ImageProcessor>>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var appPathsMock = new Mock<MediaBrowser.Controller.IServerApplicationPaths>();
            var imageEncoderMock = new Mock<IImageEncoder>();
            var configMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            configMock.Setup(c => c.Configuration).Returns(new MediaBrowser.Controller.Configuration.ServerConfiguration { ParallelImageEncodingLimit = 1 });

            var processor = new ImageProcessor(loggerMock.Object, appPathsMock.Object, fileSystemMock.Object, imageEncoderMock.Object, configMock.Object);

            var item = new BaseItem();
            var info = new ItemImageInfo
            {
                Width = 0,
                Height = 0,
                Path = "testpath"
            };

            var expectedDimensions = new ImageDimensions(123, 456);
            imageEncoderMock.Setup(x => x.GetImageSize("testpath")).Returns(expectedDimensions);

            // Act
            var result = processor.GetImageDimensions(item, info);

            // Assert
            Assert.Equal(expectedDimensions.Width, result.Width);
            Assert.Equal(expectedDimensions.Height, result.Height);
            Assert.Equal(expectedDimensions.Width, info.Width);
            Assert.Equal(expectedDimensions.Height, info.Height);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting image size for item BaseItem testpath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
