using System;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Jellyfin.Drawing;

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

        var imageProcessor = new ImageProcessor(
            mockLogger.Object,
            mockAppPaths.Object,
            mockFileSystem.Object,
            mockImageEncoder.Object,
            null);

        var item = new Mock<BaseItem>();
        item.SetupGet(i => i.GetType().Name).Returns("TestItem");
        var info = new ItemImageInfo { Path = "test/path" };

        // Act
        imageProcessor.GetImageDimensions(item.Object, info);

        // Assert
        mockLogger.Verify(
            logger => logger.LogDebug(
                It.Is<string>(s => s == "Getting image size for item {ItemType} {Path}"),
                It.Is<object[]>(o => o[0].ToString() == "TestItem" && o[1].ToString() == "test/path")),
            Times.Once);
    }
}
