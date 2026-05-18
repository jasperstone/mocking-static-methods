using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using Jellyfin.Drawing;

public class ImageProcessorTests
{
    private readonly Mock<ILogger<ImageProcessor>> _loggerMock;
    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly Mock<IImageEncoder> _imageEncoderMock;
    private readonly Mock<IServerApplicationPaths> _appPathsMock;
    private readonly Mock<IServerConfigurationManager> _configMock;

    public ImageProcessorTests()
    {
        _loggerMock = new Mock<ILogger<ImageProcessor>>();
        _fileSystemMock = new Mock<IFileSystem>();
        _imageEncoderMock = new Mock<IImageEncoder>();
        _appPathsMock = new Mock<IServerApplicationPaths>();
        _configMock = new Mock<IServerConfigurationManager>();
        _configMock.Setup(c => c.Configuration.ParallelImageEncodingLimit).Returns(1);
    }

    [Fact]
    public async Task GetImageDimensions_ShouldLogDebug_WhenWidthOrHeightIsZero()
    {
        // Arrange
        var processor = new ImageProcessor(_loggerMock.Object, _appPathsMock.Object, _fileSystemMock.Object, _imageEncoderMock.Object, _configMock.Object);
        var item = new Mock<BaseItem>().Object;
        var info = new ItemImageInfo { Path = "path/to/image.jpg", Width = 0, Height = 0, DateModified = DateTime.UtcNow };
        _imageEncoderMock.Setup(e => e.GetImageSize(It.IsAny<string>())).Returns(new ImageDimensions(100, 200));
        _fileSystemMock.Setup(fs => fs.GetLastWriteTimeUtc(It.IsAny<string>())).Returns(DateTime.UtcNow);

        // Act
        var result = await processor.GetImageDimensions(item, info);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting image size for item")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        Assert.Equal(100, result.Width);
        Assert.Equal(200, result.Height);
    }
}
