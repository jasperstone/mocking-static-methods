using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Controller.Drawing;
using Jellyfin.Drawing;

public class ImageProcessorTests
{
    [Fact]
    public void GetImageDimensions_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ImageProcessor>>();
        var imageEncoderMock = new Mock<IImageEncoder>();
        var imageProcessor = new ImageProcessor(loggerMock.Object, null, null, imageEncoderMock.Object, null);

        var item = new BaseItem();
        var info = new ItemImageInfo { Path = "testPath" };

        // Act
        imageProcessor.GetImageDimensions(item, info);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting image size for item")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
