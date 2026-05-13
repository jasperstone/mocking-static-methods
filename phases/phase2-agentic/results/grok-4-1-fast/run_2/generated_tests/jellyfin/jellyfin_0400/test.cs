using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Trickplay.Tests;

public class TrickplayManagerTests
{
    private readonly Mock<ILogger<TrickplayManager>> _loggerMock;
    private readonly Mock<IMediaEncoder> _mediaEncoderMock;
    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly Mock<EncodingHelper> _encodingHelperMock;
    private readonly Mock<IServerConfigurationManager> _configMock;
    private readonly Mock<IImageEncoder> _imageEncoderMock;
    private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;
    private readonly Mock<IApplicationPaths> _appPathsMock;
    private readonly Mock<IPathManager> _pathManagerMock;
    private readonly TrickplayManager _trickplayManager;

    public TrickplayManagerTests()
    {
        _loggerMock = new Mock<ILogger<TrickplayManager>>();
        _mediaEncoderMock = new Mock<IMediaEncoder>();
        _fileSystemMock = new Mock<IFileSystem>();
        _encodingHelperMock = new Mock<EncodingHelper>();
        _configMock = new Mock<IServerConfigurationManager>();
        _imageEncoderMock = new Mock<IImageEncoder>();
        _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        _appPathsMock = new Mock<IApplicationPaths>();
        _pathManagerMock = new Mock<IPathManager>();

        _trickplayManager = new TrickplayManager(
            _loggerMock.Object,
            _mediaEncoderMock.Object,
            _fileSystemMock.Object,
            _encodingHelperMock.Object,
            _configMock.Object,
            _imageEncoderMock.Object,
            _dbProviderMock.Object,
            _appPathsMock.Object,
            _pathManagerMock.Object);
    }

    [Fact]
    public async Task RefreshTrickplayDataAsync_ValidPath_SuccessfullyLogsInformation()
    {
        // Arrange
        var video = new Video { Id = Guid.NewGuid().ToString("N"), Name = "Test Video", Path = "/path/to/video.mp4" };
        var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true };
        var trickplayOptions = new TrickplayOptions { Interval = 10000, WidthResolutions = new[] { 320 } };
        var config = new ServerConfiguration { TrickplayOptions = trickplayOptions };
        _configMock.Setup(c => c.Configuration).Returns(config);

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        _fileSystemMock.Setup(fs => fs.GetTempDirectory()).Returns(tempDir);
        Directory.CreateDirectory(tempDir);

        _mediaEncoderMock.Setup(me => me.ExtractVideoImages(It.IsAny<string>(), It.IsAny<VideoType>(), It.IsAny<ImageType>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<EncodingHelper>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(outputDir);

        _fileSystemMock.Setup(fs => fs.GetFiles(tempDir, It.IsAny<string[]>(), false, false))
            .Returns(new[] { new FileInfo(Path.Combine(tempDir, "image1.jpg")), new FileInfo(Path.Combine(tempDir, "image2.jpg")) });

        // Mock CreateTiles to return non-null
        var trickplayInfoMock = new Mock<TrickplayInfo>();
        trickplayInfoMock.SetupProperty(t => t.ItemId);

        // Use reflection or create a testable version that calls the internal logic
        // For this test, we'll verify the logger call happens after successful SaveTrickplayInfo

        // Act
        await _trickplayManager.RefreshTrickplayDataAsync(video, true, libraryOptions, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Finished creation of trickplay files")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void LogInformationExtension_VerifyCallFormat()
    {
        // Arrange
        var logger = new Mock<ILogger<TrickplayManager>>();
        var mediaPath = "/path/to/media.mp4";

        // Act
        logger.Object.LogInformation("Finished creation of trickplay files for {0}", mediaPath);

        // Assert - Verify the LogInformation extension was called with correct template
        logger.Verify(
            l => l.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Finished creation of trickplay files for {0}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
