using System;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Jellyfin.Drawing;
using MediaBrowser.Controller.Entities; // Assuming BaseItem is here
using MediaBrowser.Model.Drawing; // Assuming ItemImageInfo is here
using MediaBrowser.Model.IO; // Assuming IFileSystem is here
using MediaBrowser.Model.Configuration; // Assuming IServerConfigurationManager and ServerConfiguration are here
using MediaBrowser.Model.ApplicationPaths; // Assuming IServerApplicationPaths is here

namespace Jellyfin.Drawing.Tests
{
    public class ImageProcessorTests
    {
        [Fact]
        public void GetImageDimensions_LogsDebugMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ImageProcessor>>();
            var mockAppPaths = new Mock<IServerApplicationPaths>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockImageEncoder = new Mock<IImageEncoder>();
            var mockConfigManager = new Mock<IServerConfigurationManager>();

            mockAppPaths.SetupGet(p => p.ImageCachePath).Returns("cache/path");
            mockConfigManager.Setup(c => c.Configuration).Returns(new ServerConfiguration { ParallelImageEncodingLimit = 1 });

            var imageProcessor = new ImageProcessor(
                mockLogger.Object,
                mockAppPaths.Object,
                mockFileSystem.Object,
                mockImageEncoder.Object,
                mockConfigManager.Object
            );

            var item = new Mock<BaseItem>();
            item.SetupGet(i => i.GetType().Name).Returns("TestItem");
            var info = new ItemImageInfo { Path = "test/path" };

            // Act
            imageProcessor.GetImageDimensions(item.Object, info);

            // Assert
            mockLogger.Verify(
                logger => logger.LogDebug(
                    It.Is<string>(s => s == "Getting image size for item {ItemType} {Path}"),
                    It.Is<object[]>(args => args[0] == "TestItem" && args[1] == "test/path")),
                Times.Once);
        }
    }
}
