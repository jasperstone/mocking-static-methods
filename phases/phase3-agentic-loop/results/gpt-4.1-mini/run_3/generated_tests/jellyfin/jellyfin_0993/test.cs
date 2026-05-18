using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Drawing;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Controller.Entities;

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        [Fact]
        public void GetImageDimensions_LogsDebug_WhenWidthOrHeightIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ImageProcessor>>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var appPathsMock = new Mock<MediaBrowser.Controller.IServerApplicationPaths>();
            var imageEncoderMock = new Mock<Jellyfin.Drawing.IImageEncoder>();
            var configMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            configMock.Setup(c => c.Configuration).Returns(new MediaBrowser.Model.Configuration.ServerConfiguration { ParallelImageEncodingLimit = 1 });

            var imageProcessor = new ImageProcessor(
                loggerMock.Object,
                appPathsMock.Object,
                fileSystemMock.Object,
                imageEncoderMock.Object,
                configMock.Object);

            var itemMock = new Mock<BaseItem>();
            itemMock.Setup(i => i.GetType().Name).Returns("TestItem");

            var info = new ItemImageInfo
            {
                Width = 0,
                Height = 0,
                Path = "testpath"
            };

            // Setup imageEncoder to return a fixed size for GetImageDimensions(string)
            imageEncoderMock.Setup(e => e.GetImageSize("testpath"))
                .Returns(new ImageDimensions(100, 200));

            // Act
            var result = imageProcessor.GetImageDimensions(itemMock.Object, info);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting image size for item TestItem testpath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Equal(100, result.Width);
            Assert.Equal(200, result.Height);
            Assert.Equal(100, info.Width);
            Assert.Equal(200, info.Height);
        }

        [Fact]
        public void GetImageDimensions_ReturnsDimensionsDirectly_WhenWidthAndHeightArePositive()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ImageProcessor>>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var appPathsMock = new Mock<MediaBrowser.Controller.IServerApplicationPaths>();
            var imageEncoderMock = new Mock<Jellyfin.Drawing.IImageEncoder>();
            var configMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            configMock.Setup(c => c.Configuration).Returns(new MediaBrowser.Model.Configuration.ServerConfiguration { ParallelImageEncodingLimit = 1 });

            var imageProcessor = new ImageProcessor(
                loggerMock.Object,
                appPathsMock.Object,
                fileSystemMock.Object,
                imageEncoderMock.Object,
                configMock.Object);

            var itemMock = new Mock<BaseItem>();
            var info = new ItemImageInfo
            {
                Width = 123,
                Height = 456,
                Path = "somepath"
            };

            // Act
            var result = imageProcessor.GetImageDimensions(itemMock.Object, info);

            // Assert
            // Should not log debug because width and height are positive
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);

            Assert.Equal(123, result.Width);
            Assert.Equal(456, result.Height);
        }
    }
}
