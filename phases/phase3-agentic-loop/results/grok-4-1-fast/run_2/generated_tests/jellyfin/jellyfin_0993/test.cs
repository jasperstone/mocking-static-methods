using System;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Model.Drawing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Drawing.Tests;

public class ImageProcessorTests
{
    private class MockLogger<T> : ILogger<T>
    {
        public IReadOnlyList<(string Message, object[] Args)> DebugMessages { get; } = new List<(string, object[])>();

        public IDisposable? BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Debug)
            {
                var message = formatter(state, exception);
                DebugMessages.Add((message, Array.Empty<object>()));
            }
        }
    }

    [Fact]
    public void GetImageDimensions_LogsDebug_WhenWidthAndHeightAreZero()
    {
        // Arrange
        var logger = new MockLogger<ImageProcessor>();
        var fileSystem = new Mock<MediaBrowser.Model.IO.IFileSystem>();
        var appPaths = new Mock<MediaBrowser.Controller.Configuration.IServerApplicationPaths>();
        var imageEncoder = new Mock<IImageEncoder>();
        var config = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
        
        imageEncoder.Setup(e => e.GetImageSize(It.IsAny<string>())).Returns(new ImageDimensions(1920, 1080));

        var processor = new ImageProcessor(logger, appPaths.Object, fileSystem.Object, imageEncoder.Object, config.Object);

        var item = new Mock<MediaBrowser.Controller.Entities.BaseItem>();
        item.Setup(i => i.GetType()).Returns(typeof(MediaBrowser.Controller.Entities.BaseItem));
        
        var info = new MediaBrowser.Model.Dto.DtoBaseItem.ImageInfo
        {
            Path = "/test/image.jpg",
            Width = 0,
            Height = 0
        };

        // Act
        var result = processor.GetImageDimensions(item.Object, info);

        // Assert - Verify the exact LogDebug call
        var debugMessage = Assert.Single(logger.DebugMessages);
        Assert.StartsWith("Getting image size for item BaseItem /test/image.jpg", debugMessage.Message);

        Assert.Equal(1920, result.Width);
        Assert.Equal(1080, result.Height);
        Assert.Equal(1920, info.Width);
        Assert.Equal(1080, info.Height);
    }

    [Fact]
    public void GetImageDimensions_NoLog_WhenWidthAndHeightAreSet()
    {
        // Arrange
        var logger = new MockLogger<ImageProcessor>();
        var fileSystem = new Mock<MediaBrowser.Model.IO.IFileSystem>();
        var appPaths = new Mock<MediaBrowser.Controller.Configuration.IServerApplicationPaths>();
        var imageEncoder = new Mock<IImageEncoder>();
        var config = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();

        var processor = new ImageProcessor(logger, appPaths.Object, fileSystem.Object, imageEncoder.Object, config.Object);

        var item = new Mock<MediaBrowser.Controller.Entities.BaseItem>();
        var info = new MediaBrowser.Model.Dto.DtoBaseItem.ImageInfo
        {
            Path = "/test/image.jpg",
            Width = 1920,
            Height = 1080
        };

        // Act
        var result = processor.GetImageDimensions(item.Object, info);

        // Assert - No debug log when dimensions are already set
        Assert.Empty(logger.DebugMessages);

        Assert.Equal(1920, result.Width);
        Assert.Equal(1080, result.Height);
    }
}
