using System;
using System.Threading.Tasks;
using Jellyfin.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Drawing;

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        [Fact]
        public void GetImageDimensions_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ImageProcessor>>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var appPathsMock = new Mock<MediaBrowser.Controller.IServerApplicationPaths>();
            var imageEncoderMock = new Mock<IImageEncoder>();
            var configMock = new Mock<IServerConfigurationManager>();
            var serverConfig = new ServerConfiguration { ParallelImageEncodingLimit = 1 };
            configMock.Setup(c => c.Configuration).Returns(serverConfig);

            var processor = new ImageProcessor(
                loggerMock.Object,
                appPathsMock.Object,
                fileSystemMock.Object,
                imageEncoderMock.Object,
                configMock.Object);

            var item = new TestBaseItem();
            var info = new ItemImageInfo
            {
                Path = "testpath",
                Width = 0,
                Height = 0
            };

            var expectedDimensions = new ImageDimensions(100, 200);
            imageEncoderMock.Setup(e => e.GetImageSize("testpath")).Returns(expectedDimensions);

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting image size for item TestBaseItem testpath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestBaseItem : BaseItem
        {
        }
    }
}
