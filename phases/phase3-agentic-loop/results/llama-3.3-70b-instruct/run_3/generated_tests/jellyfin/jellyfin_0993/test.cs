using Xunit;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Jellyfin.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Drawing;

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
            var appPathsMock = new Mock<MediaBrowser.Controller.ServerApplicationPaths>();
            var imageEncoderMock = new Mock<MediaBrowser.Controller.Drawing.IImageEncoder>();
            var configMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();

            var imageProcessor = new ImageProcessor(
                loggerMock.Object,
                appPathsMock.Object,
                fileSystemMock.Object,
                imageEncoderMock.Object,
                configMock.Object);

            var item = new MediaBrowser.Controller.Entities.BaseItem { };
            var info = new MediaBrowser.Model.Entities.ItemImageInfo { Path = "path" };

            // Act
            await imageProcessor.GetImageDimensions(item, info);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Getting image size for item {ItemType} {Path}", item.GetType().Name, info.Path), Times.Once);
        }
    }
}
