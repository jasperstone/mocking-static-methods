using System;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Jellyfin.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;

public class ImageProcessorTests
{
    [Fact]
    public void GetImageDimensions_LogsDebugMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ImageProcessor>>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockAppPaths = new Mock<IServerApplicationPaths>();
        var mockImageEncoder = new Mock<IImageEncoder>();
        var mockConfig = new Mock<IServerConfigurationManager>();

        var imageProcessor = new ImageProcessor(
            mockLogger.Object,
            mockAppPaths.Object,
            mockFileSystem.Object,
            mockImageEncoder.Object,
            mockConfig.Object);

        var item = new Mock<BaseItem>();
        item.SetupGet(i => i.GetType().Name).Returns("MockItem");
        var info = new ItemImageInfo { Path = "mock/path" };

        // Act
        imageProcessor.GetImageDimensions(item.Object, info);

        // Assert
        mockLogger.Verify(
            logger => logger.LogDebug(
                It.Is<string>(s => s.Contains("Getting image size for item MockItem mock/path")),
                It.IsAny<object>(),
                It.IsAny<object>()), Times.Once);
    }
}
