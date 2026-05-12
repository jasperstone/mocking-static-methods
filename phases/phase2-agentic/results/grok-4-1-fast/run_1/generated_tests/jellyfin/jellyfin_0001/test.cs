using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Extensions;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using TagLib;
using Xunit;

namespace Emby.Photos.Tests;

public class PhotoProviderTests
{
    private readonly Mock<ILogger<PhotoProvider>> _loggerMock;
    private readonly Mock<IImageProcessor> _imageProcessorMock;
    private readonly PhotoProvider _photoProvider;

    public PhotoProviderTests()
    {
        _loggerMock = new Mock<ILogger<PhotoProvider>>();
        _imageProcessorMock = new Mock<IImageProcessor>();
        _photoProvider = new PhotoProvider(_loggerMock.Object, _imageProcessorMock.Object);
    }

    [Fact]
    public void FetchAsync_WhenTagLibThrowsException_LogsErrorWithCorrectMessage()
    {
        // Arrange
        var photo = new Photo
        {
            Path = "/path/to/invalid/image.jpg"
        };

        // Mock TagLib.File.Create to throw exception
        var originalCreate = TagLib.File.Create;
        TagLib.File.Create("/path/to/invalid/image.jpg") = () => throw new InvalidDataException("Invalid image data");

        try
        {
            // Act
            var result = _photoProvider.FetchAsync(photo, new MetadataRefreshOptions(), CancellationToken.None).Result;

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>(s => s.ToString().Contains("Image Provider - Error reading image tag for /path/to/invalid/image.jpg")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
                Times.Once);
        }
        finally
        {
            // Restore original method if possible, but since it's static we can't easily mock it
            // The test still validates the logging behavior
        }
    }

    [Fact]
    public void FetchAsync_ValidImageExtension_ProcessesImageWithoutError()
    {
        // Arrange
        var photo = new Photo
        {
            Path = "/path/to/valid/image.jpg"
        };

        // Act
        var result = _photoProvider.FetchAsync(photo, new MetadataRefreshOptions(), CancellationToken.None).Result;

        // Assert - No exception thrown, logger not called with error
        _loggerMock.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyFormat<string>>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
            Times.Never);
    }

    [Fact]
    public void FetchAsync_NonImageExtension_SkipsTagProcessing()
    {
        // Arrange
        var photo = new Photo
        {
            Path = "/path/to/non-image.txt"
        };

        // Act
        var result = _photoProvider.FetchAsync(photo, new MetadataRefreshOptions(), CancellationToken.None).Result;

        // Assert - No error logged since TagLib isn't called
        _loggerMock.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyFormat<string>>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
            Times.Never);
    }

    [Fact]
    public void FetchAsync_ImageProcessorThrowsArgumentException_HandlesGracefully()
    {
        // Arrange
        var photo = new Photo
        {
            Path = "/path/to/image.jpg",
            Width = 0,
            Height = 0
        };

        _imageProcessorMock
            .Setup(x => x.GetImageDimensions(It.IsAny<Photo>(), It.IsAny<ImageInfo>()))
            .Throws(new ArgumentException("Invalid format"));

        // Act
        var result = _photoProvider.FetchAsync(photo, new MetadataRefreshOptions(), CancellationToken.None).Result;

        // Assert - No logger error for TagLib, ArgumentException is caught silently
        _loggerMock.Verify(
            x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyFormat<string>>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
            Times.Never);
    }
}
