using System;
using Jellyfin.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Drawing.Tests
{
    // Minimal stub for IImageEncoder to allow mocking
    public interface IImageEncoder
    {
        ImageDimensions GetImageSize(string path);
    }

    public class ImageProcessorTests
    {
        [Fact]
        public void GetImageDimensions_LogsDebugAndReturnsSize_WhenWidthOrHeightIsZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ImageProcessor>>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var appPathsMock = new Mock<MediaBrowser.Controller.IServerApplicationPaths>();
            var imageEncoderMock = new Mock<IImageEncoder>();
            var configMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            configMock.Setup(c => c.Configuration).Returns(new MediaBrowser.Model.Configuration.ServerConfiguration { ParallelImageEncodingLimit = 1 });

            var imageProcessor = new ImageProcessor(
                loggerMock.Object,
                appPathsMock.Object,
                fileSystemMock.Object,
                imageEncoderMock.Object,
                configMock.Object);

            var item = new TestBaseItem();
            var info = new ItemImageInfo
            {
                Width = 0,
                Height = 0,
                Path = "testpath.jpg"
            };

            var expectedDimensions = new ImageDimensions(123, 456);
            imageEncoderMock.Setup(e => e.GetImageSize("testpath.jpg")).Returns(expectedDimensions);

            // Act
            var result = imageProcessor.GetImageDimensions(item, info);

            // Assert
            Assert.Equal(expectedDimensions.Width, result.Width);
            Assert.Equal(expectedDimensions.Height, result.Height);
            Assert.Equal(expectedDimensions.Width, info.Width);
            Assert.Equal(expectedDimensions.Height, info.Height);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting image size for item TestBaseItem testpath.jpg")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private class TestBaseItem : BaseItem
        {
            public override string Name => "TestBaseItem";
        }
    }
}
