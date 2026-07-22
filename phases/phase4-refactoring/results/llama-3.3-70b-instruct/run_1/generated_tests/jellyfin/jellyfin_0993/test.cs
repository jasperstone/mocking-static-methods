using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        [Fact]
        public async Task GetImageDimensions_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ImageProcessor>>();
            var fileSystemMock = new Mock<MediaBrowser.Common.IO.IFileSystem>();
            var appPathsMock = new Mock<MediaBrowser.Common.ApplicationPaths.IServerApplicationPaths>();
            var imageEncoderMock = new Mock<MediaBrowser.Controller.Drawing.IImageEncoder>();
            var configMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();

            var imageProcessor = new ImageProcessor(
                loggerMock.Object,
                appPathsMock.Object,
                fileSystemMock.Object,
                imageEncoderMock.Object,
                configMock.Object);

            var item = new MediaBrowser.Controller.Entities.Movie { };
            var info = new MediaBrowser.Controller.Entities.ItemImageInfo { Path = "path" };

            // Act
            var result = imageProcessor.GetImageDimensions(item, info);

            // Assert
            loggerMock.Verify(logger => logger.LogDebug("Getting image size for item {ItemType} {Path}", item.GetType().Name, info.Path), Times.Once);
        }
    }
}
