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

        var item = new BaseItem(); // Assuming BaseItem has a default constructor
        var info = new ItemImageInfo
        {
            Path = "test/path.jpg",
            Width = 0,
            Height = 0
        };

        // Act
        imageProcessor.GetImageDimensions(item, info);

        // Assert
        mockLogger.Verify(
            logger => logger.LogDebug(
                It.Is<string>(s => s == "Getting image size for item {ItemType} {Path}"),
                It.Is<object[]>(o => o[0] == item.GetType().Name && o[1] == info.Path),
                It.IsAny<ILoggerLogOptions>(),
                It.IsAny<Exception>()
            ),
            Times.Once
        );
    }
}
