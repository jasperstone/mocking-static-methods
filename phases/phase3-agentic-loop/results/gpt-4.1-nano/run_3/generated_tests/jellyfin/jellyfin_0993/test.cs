using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.IO;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Configuration;
using Jellyfin.Drawing;

public class ImageProcessorTests
{
    [Fact]
    public async Task GetImageDimensions_ShouldLogDebug_WhenCalledWithNullDimensions()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ImageProcessor>>();
        var fileSystemMock = new Mock<IFileSystem>();
        var appPathsMock = new Mock<IServerApplicationPaths>();
        var imageEncoderMock = new Mock<IImageEncoder>();
        var configMock = new Mock<IServerConfigurationManager>();

        // Setup configuration
        var config = new ServerConfiguration { ParallelImageEncodingLimit = 1 };
        configMock.Setup(c => c.Configuration).Returns(config);

        var processor = new ImageProcessor(
            loggerMock.Object,
            appPathsMock.Object,
            fileSystemMock.Object,
            imageEncoderMock.Object,
            configMock.Object);

        var item = new Mock<BaseItem>().Object;
        var info = new ItemImageInfo
        {
            Width = 0,
            Height = 0,
            Path = "test.jpg",
            DateModified = DateTime.UtcNow
        };

        // Act
        var result = await processor.GetImageDimensions(item, info);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Getting image size for item")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
