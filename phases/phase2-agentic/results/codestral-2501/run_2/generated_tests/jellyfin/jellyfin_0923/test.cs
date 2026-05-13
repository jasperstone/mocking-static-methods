using Xunit;
using MediaBrowser.Providers.MediaInfo;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ProbeProviderTests
{
    private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
    private readonly Mock<ILibraryManager> _libraryManagerMock;
    private readonly Mock<IDirectoryService> _directoryServiceMock;
    private readonly Mock<ISubtitleResolver> _subtitleResolverMock;
    private readonly Mock<IAudioResolver> _audioResolverMock;
    private readonly Mock<ILyricResolver> _lyricResolverMock;
    private readonly ProbeProvider _probeProvider;

    public ProbeProviderTests()
    {
        _loggerMock = new Mock<ILogger<ProbeProvider>>();
        _libraryManagerMock = new Mock<ILibraryManager>();
        _directoryServiceMock = new Mock<IDirectoryService>();
        _subtitleResolverMock = new Mock<ISubtitleResolver>();
        _audioResolverMock = new Mock<IAudioResolver>();
        _lyricResolverMock = new Mock<ILyricResolver>();

        _probeProvider = new ProbeProvider(
            _libraryManagerMock.Object,
            _directoryServiceMock.Object,
            _subtitleResolverMock.Object,
            _audioResolverMock.Object,
            _lyricResolverMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void HasChanged_ShouldLogDebug_WhenExternalLyricsChange()
    {
        // Arrange
        var audio = new Audio
        {
            LyricFiles = new[] { "lyric1.txt" },
            SupportsLocalMetadata = true
        };

        var externalLyrics = new List<FileSystemMetadata>
        {
            new FileSystemMetadata { Path = "lyric2.txt" }
        };

        _lyricResolverMock.Setup(x => x.GetExternalFiles(audio, _directoryServiceMock.Object, false))
            .Returns(externalLyrics);

        // Act
        var result = _probeProvider.HasChanged(audio, _directoryServiceMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.LogDebug("Refreshing {ItemPath} due to external lyrics change.", audio.Path),
            Times.Once);
        Assert.True(result);
    }

    [Fact]
    public void HasChanged_ShouldLogDebug_WhenExternalSubtitlesChange()
    {
        // Arrange
        var video = new Video
        {
            SubtitleFiles = new[] { "subtitle1.srt" },
            SupportsLocalMetadata = true,
            IsPlaceHolder = false
        };

        var externalSubtitles = new List<FileSystemMetadata>
        {
            new FileSystemMetadata { Path = "subtitle2.srt" }
        };

        _subtitleResolverMock.Setup(x => x.GetExternalFiles(video, _directoryServiceMock.Object, false))
            .Returns(externalSubtitles);

        // Act
        var result = _probeProvider.HasChanged(video, _directoryServiceMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.LogDebug("Refreshing {ItemPath} due to external subtitles change.", video.Path),
            Times.Once);
        Assert.True(result);
    }

    [Fact]
    public void HasChanged_ShouldLogDebug_WhenExternalAudioChange()
    {
        // Arrange
        var video = new Video
        {
            AudioFiles = new[] { "audio1.mp3" },
            SupportsLocalMetadata = true,
            IsPlaceHolder = false
        };

        var externalAudios = new List<FileSystemMetadata>
        {
            new FileSystemMetadata { Path = "audio2.mp3" }
        };

        _audioResolverMock.Setup(x => x.GetExternalFiles(video, _directoryServiceMock.Object, false))
            .Returns(externalAudios);

        // Act
        var result = _probeProvider.HasChanged(video, _directoryServiceMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.LogDebug("Refreshing {ItemPath} due to external audio change.", video.Path),
            Times.Once);
        Assert.True(result);
    }

    [Fact]
    public void HasChanged_ShouldLogDebug_WhenFileSystemModification()
    {
        // Arrange
        var video = new Video
        {
            Path = "video.mp4",
            IsFileProtocol = true
        };

        var file = new FileSystemMetadata
        {
            LastWriteTimeUtc = DateTime.UtcNow
        };

        _directoryServiceMock.Setup(x => x.GetFile(video.Path))
            .Returns(file);

        // Act
        var result = _probeProvider.HasChanged(video, _directoryServiceMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.LogDebug("Refreshing {ItemPath} due to file system modification.", video.Path),
            Times.Once);
        Assert.True(result);
    }
}
