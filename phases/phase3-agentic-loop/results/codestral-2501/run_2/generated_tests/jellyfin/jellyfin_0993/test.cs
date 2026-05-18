using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;

public class ImageProcessorTests
{
    [Fact]
    public void GetImageDimensions_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ImageProcessor>>();
        var imageEncoderMock = new Mock<IImageEncoder>();
        var item = new BaseItem();
        var info = new ItemImageInfo { Path = "testPath" };

        var imageProcessor = new ImageProcessor(
            loggerMock.Object,
            null,
            null,
            imageEncoderMock.Object,
            null);

        // Act
        imageProcessor.GetImageDimensions(item, info);

        // Assert
        loggerMock.Verify(
            x => x.LogDebug(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
