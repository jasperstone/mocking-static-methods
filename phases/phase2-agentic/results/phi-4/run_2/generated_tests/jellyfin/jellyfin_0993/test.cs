using System;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Jellyfin.Drawing;

public class ImageProcessorTests
{
    [Fact]
    public void GetImageDimensions_LogsDebugMessage_WhenDimensionsAreNotProvided()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ImageProcessor>>();
        var imageEncoderMock = new Mock<IImageEncoder>();
        var fileSystemMock = new Mock<IFileSystem>();
        var appPathsMock = new Mock<IServerApplicationPaths>();

        imageEncoderMock
            .Setup(e => e.GetImageSize(It.IsAny<string>()))
            .Returns(new ImageDimensions(1920, 1080));

        var imageProcessor = new ImageProcessor(
            loggerMock.Object,
            appPathsMock.Object,
            fileSystemMock.Object,
            imageEncoderMock.Object,
            null);

        var item = new BaseItem(); // Assuming BaseItem is a valid class
        var info = new ItemImageInfo
        {
            Width = 0,
            Height = 0,
            Path = "test.jpg"
        };

        // Act
        var dimensions = imageProcessor.GetImageDimensions(item, info);

        // Assert
        loggerMock.Verify(
            l => l.LogDebug(
                It.Is<string>(s => s == "Getting image size for item {ItemType} {Path}"),
                It.Is<object[]>(o => o[0] == item.GetType().Name && o[1] == "test.jpg")),
            Times.Once);

        Assert.Equal(1920, dimensions.Width);
        Assert.Equal(1080, dimensions.Height);
        Assert.Equal(1920, info.Width);
        Assert.Equal(1080, info.Height);
    }
}
