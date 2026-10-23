using System;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Drawing.Tests;

public class ImageProcessorTests
{
    [Fact]
    public void GetImageDimensions_WithZeroWidthAndHeight_LogsDebugMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ImageProcessor>>();
        mockLogger.Setup(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Getting image size for item")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        var mockEncoder = new Mock<IImageEncoder>();
        mockEncoder.Setup(x => x.GetImageSize(It.IsAny<string>()))
            .Returns(new ImageDimensions(100, 100));

        var mockConfig = new Mock<IServerConfigurationManager>();
        mockConfig.Setup(x => x.Configuration)
            .Returns(new ServerConfiguration { ParallelImageEncodingLimit = 1 });

        var item = new Mock<BaseItem>().Object;
        var info = new ItemImageInfo
        {
            Path = "/path/to/image.jpg",
            Width = 0,
            Height = 0
        };

        var imageProcessor = new ImageProcessor(
            mockLogger.Object,
            Mock.Of<IServerApplicationPaths>(),
            Mock.Of<IFileSystem>(),
            mockEncoder.Object,
            mockConfig.Object);

        // Act
        imageProcessor.GetImageDimensions(item, info);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetImageDimensions_WithPositiveWidthAndHeight_DoesNotLogDebugMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ImageProcessor>>();

        var mockConfig = new Mock<IServerConfigurationManager>();
        mockConfig.Setup(x => x.Configuration)
            .Returns(new ServerConfiguration { ParallelImageEncodingLimit = 1 });

        var item = new Mock<BaseItem>().Object;
        var info = new ItemImageInfo
        {
            Path = "/path/to/image.jpg",
            Width = 800,
            Height = 600
        };

        var imageProcessor = new ImageProcessor(
            mockLogger.Object,
            Mock.Of<IServerApplicationPaths>(),
            Mock.Of<IFileSystem>(),
            Mock.Of<IImageEncoder>(),
            mockConfig.Object);

        // Act
        imageProcessor.GetImageDimensions(item, info);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
