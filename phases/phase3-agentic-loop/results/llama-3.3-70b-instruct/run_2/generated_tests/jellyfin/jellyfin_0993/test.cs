using Xunit;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Jellyfin.Drawing;

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        [Fact]
        public async Task GetImageDimensions_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ImageProcessor>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var imageEncoderMock = new Mock<IImageEncoder>();
            var configMock = new Mock<IServerConfigurationManager>();

            var imageProcessor = new ImageProcessor(
                loggerMock.Object,
                appPathsMock.Object,
                fileSystemMock.Object,
                imageEncoderMock.Object,
                configMock.Object);

            var item = new object();
            var info = new ItemImageInfo { Path = "path" };

            // Act
            await imageProcessor.GetImageDimensions(item, info);

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()), Times.Once);
        }
    }
}
