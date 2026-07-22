using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using Jellyfin.Drawing;

public class ImageProcessorTests
{
    [Fact]
    public void GetImageDimensions_LogsDebugMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ImageProcessor>>();
        var imageEncoderMock = new Mock<IImageEncoder>();
        var fileSystemMock = new Mock<IFileSystem>();
        var appPathsMock = new Mock<IServerApplicationPaths>();
        var configMock = new Mock<IServerConfigurationManager>();

        var imageProcessor = new ImageProcessor(
            loggerMock.Object,
            appPathsMock.Object,
            fileSystemMock.Object,
            imageEncoderMock.Object,
            configMock.Object);

        var item = new BaseItem();
        var info = new ItemImageInfo { Path = "testPath" };

        // Act
        imageProcessor.GetImageDimensions(item, info);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting image size for item")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
