using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.MediaEncoding.Subtitles;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class SubtitleEncoderTests
{
    private readonly Mock<ILogger<SubtitleEncoder>> _loggerMock;
    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly Mock<IMediaEncoder> _mediaEncoderMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IMediaSourceManager> _mediaSourceManagerMock;
    private readonly Mock<ISubtitleParser> _subtitleParserMock;
    private readonly Mock<IPathManager> _pathManagerMock;
    private readonly Mock<IServerConfigurationManager> _serverConfigurationManagerMock;
    private readonly SubtitleEncoder _subtitleEncoder;

    public SubtitleEncoderTests()
    {
        _loggerMock = new Mock<ILogger<SubtitleEncoder>>();
        _fileSystemMock = new Mock<IFileSystem>();
        _mediaEncoderMock = new Mock<IMediaEncoder>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _mediaSourceManagerMock = new Mock<IMediaSourceManager>();
        _subtitleParserMock = new Mock<ISubtitleParser>();
        _pathManagerMock = new Mock<IPathManager>();
        _serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();

        _subtitleEncoder = new SubtitleEncoder(
            _loggerMock.Object,
            _fileSystemMock.Object,
            _mediaEncoderMock.Object,
            _httpClientFactoryMock.Object,
            _mediaSourceManagerMock.Object,
            _subtitleParserMock.Object,
            _pathManagerMock.Object,
            _serverConfigurationManagerMock.Object);
    }

    [Fact]
    public async Task ConvertSubtitles_ShouldLogError_WhenDeletingConvertedSubtitleFails()
    {
        // Arrange
        var outputPath = "testPath";
        _fileSystemMock.Setup(fs => fs.DeleteFile(outputPath)).Throws<IOException>();

        // Act
        await _subtitleEncoder.ConvertSubtitles(new MemoryStream(), "srt", "ass", 0, 0, false, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            logger => logger.LogError(
                It.IsAny<IOException>(),
                "Error deleting converted subtitle {Path}",
                outputPath),
            Times.Once);
    }

    [Fact]
    public async Task ConvertSubtitles_ShouldLogError_WhenDeletingConvertedSubtitleFails_WithFileNotFoundException()
    {
        // Arrange
        var outputPath = "testPath";
        _fileSystemMock.Setup(fs => fs.DeleteFile(outputPath)).Throws<FileNotFoundException>();

        // Act
        await _subtitleEncoder.ConvertSubtitles(new MemoryStream(), "srt", "ass", 0, 0, false, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            logger => logger.LogError(
                It.IsAny<FileNotFoundException>(),
                "Error deleting converted subtitle {Path}",
                outputPath),
            Times.Once);
    }

    [Fact]
    public async Task ConvertSubtitles_ShouldLogError_WhenFfmpegSubtitleConversionFails()
    {
        // Arrange
        var inputPath = "testInputPath";
        var outputPath = "testOutputPath";
        _fileSystemMock.Setup(fs => fs.GetFileInfo(outputPath)).Returns(new FileSystemMetadata { Length = 0 });

        // Act
        await Assert.ThrowsAsync<FfmpegException>(() => _subtitleEncoder.ConvertSubtitles(new MemoryStream(), "srt", "ass", 0, 0, false, CancellationToken.None));

        // Assert
        _loggerMock.Verify(
            logger => logger.LogError(
                "ffmpeg subtitle conversion failed for {Path}",
                inputPath),
            Times.Once);
    }
}
