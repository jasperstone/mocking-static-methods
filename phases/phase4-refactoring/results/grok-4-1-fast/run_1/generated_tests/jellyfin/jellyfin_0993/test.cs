using System;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.Drawing.Tests;

public class ImageProcessorTests
{
    private class FakeLogger : ILogger<ImageProcessor>
    {
        public bool DebugLogged { get; private set; }
        public string? LastMessage { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null!;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Debug)
            {
                DebugLogged = true;
                LastMessage = formatter(state, exception);
            }
        }
    }

    [Fact]
    public void GetImageDimensions_WhenWidthAndHeightAreZero_LogsDebugMessage()
    {
        // Arrange
        var logger = new FakeLogger();
        var mockItem = new Mock<BaseItem>();
        mockItem.Setup(i => i.GetType().Name).Returns("TestItem");

        var imageInfo = new ItemImageInfo
        {
            Width = 0,
            Height = 0,
            Path = "/test/path.jpg"
        };

        // Minimal mocks for constructor dependencies
        var fileSystem = new Mock<object>().Object;
        var appPaths = new Mock<object>().Object;
        var imageEncoder = new Mock<IImageEncoder>();
        imageEncoder.Setup(e => e.GetImageSize(It.IsAny<string>()))
            .Returns(new ImageDimensions(100, 100));
        var config = new Mock<object>().Object;

        var processor = new ImageProcessor(logger, appPaths, fileSystem, imageEncoder.Object, config);

        // Act
        processor.GetImageDimensions(mockItem.Object, imageInfo);

        // Assert
        Assert.True(logger.DebugLogged);
        Assert.Contains("Getting image size for item TestItem /test/path.jpg", logger.LastMessage ?? "", StringComparison.Ordinal);
    }

    [Fact]
    public void GetImageDimensions_WhenWidthAndHeightArePositive_DoesNotLogDebugMessage()
    {
        // Arrange
        var logger = new FakeLogger();
        var mockItem = new Mock<BaseItem>();

        var imageInfo = new ItemImageInfo
        {
            Width = 1920,
            Height = 1080,
            Path = "/test/path.jpg"
        };

        // Minimal mocks for constructor dependencies
        var fileSystem = new Mock<object>().Object;
        var appPaths = new Mock<object>().Object;
        var imageEncoder = new Mock<IImageEncoder>();
        var config = new Mock<object>().Object;

        var processor = new ImageProcessor(logger, appPaths, fileSystem, imageEncoder.Object, config);

        // Act
        processor.GetImageDimensions(mockItem.Object, imageInfo);

        // Assert - no debug log expected since dimensions were already set
        Assert.False(logger.DebugLogged);
    }
}
