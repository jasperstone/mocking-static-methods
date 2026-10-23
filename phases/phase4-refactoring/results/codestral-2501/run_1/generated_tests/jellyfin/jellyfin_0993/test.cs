using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.IO;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Drawing;

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        [Fact]
        public void GetImageDimensions_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ImageProcessor>>();
            var imageEncoderMock = new Mock<IImageEncoder>();
            var item = new Mock<BaseItem>().Object;
            var info = new ItemImageInfo { Path = "testPath" };
            var expectedDimensions = new ImageDimensions(100, 200);

            imageEncoderMock.Setup(ie => ie.GetImageSize(It.IsAny<string>())).Returns(expectedDimensions);

            var configManagerMock = new Mock<IServerConfigurationManager>();
            configManagerMock.Setup(cm => cm.Configuration).Returns(new ServerConfiguration { ParallelImageEncodingLimit = 4 });

            var imageProcessor = new ImageProcessor(
                loggerMock.Object,
                Mock.Of<IServerApplicationPaths>(),
                Mock.Of<IFileSystem>(),
                imageEncoderMock.Object,
                configManagerMock.Object);

            // Act
            var result = imageProcessor.GetImageDimensions(item, info);

            // Assert
            loggerMock.Verify(
                x => x.LogDebug(
                    "Getting image size for item {ItemType} {Path}",
                    It.Is<object[]>(o => o[0] == item.GetType().Name && o[1] == info.Path)),
                Times.Once);

            Assert.Equal(expectedDimensions, result);
        }
    }
}
