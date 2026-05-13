using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Drawing;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        [Fact]
        public void GetImageDimensions_LogsDebugMessage_WhenWidthOrHeightIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ImageProcessor>>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var fileSystemMock = new Mock<IFileSystem>();
            var imageEncoderMock = new Mock<IImageEncoder>();
            var configMock = new Mock<IServerConfigurationManager>();
            configMock.Setup(c => c.Configuration).Returns(new ServerConfiguration { ParallelImageEncodingLimit = 1 });

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
                Path = "testpath.jpg"
            };

            var expectedDimensions = new ImageDimensions(100, 200);
            imageEncoderMock.Setup(e => e.GetImageSize("testpath.jpg")).Returns(expectedDimensions);

            // Act
            var result = imageProcessor.GetImageDimensions(itemMock.Object, info);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting image size for item TestItem testpath.jpg")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Equal(expectedDimensions.Width, result.Width);
            Assert.Equal(expectedDimensions.Height, result.Height);
            Assert.Equal(expectedDimensions.Width, info.Width);
            Assert.Equal(expectedDimensions.Height, info.Height);
        }

        [Fact]
        public void GetImageDimensions_ReturnsDimensionsDirectly_WhenWidthAndHeightArePositive()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ImageProcessor>>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var fileSystemMock = new Mock<IFileSystem>();
            var imageEncoderMock = new Mock<IImageEncoder>();
            var configMock = new Mock<IServerConfigurationManager>();
            configMock.Setup(c => c.Configuration).Returns(new ServerConfiguration { ParallelImageEncodingLimit = 1 });

            var imageProcessor = new ImageProcessor(
                loggerMock.Object,
                appPathsMock.Object,
                fileSystemMock.Object,
                imageEncoderMock.Object,
                configMock.Object);

            var itemMock = new Mock<BaseItem>();
            var info = new ItemImageInfo
            {
                Width = 100,
                Height = 200,
                Path = "testpath.jpg"
            };

            // Act
            var result = imageProcessor.GetImageDimensions(itemMock.Object, info);

            // Assert
            // Logger should not be called because width and height are positive
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);

            Assert.Equal(100, result.Width);
            Assert.Equal(200, result.Height);
        }

        // Helper class to mock configuration
        private class ServerConfiguration : IServerConfiguration
        {
            public int ParallelImageEncodingLimit { get; set; }
        }
    }
}
