using Xunit;
using Moq;
using MediaBrowser.Providers.MediaInfo;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Entities;

public class ProbeProviderTests
{
    [Fact]
    public void LogDebug_Called_WhenExternalLyricsChange()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ProbeProvider>>();
        var subtitleResolverMock = new Mock<ISubtitleResolver>();
        var audioResolverMock = new Mock<IAudioResolver>();
        var lyricResolverMock = new Mock<ILyricResolver>();
        var directoryServiceMock = new Mock<IDirectoryService>();

        var audio = new TestAudio
        {
            LyricFiles = new List<string> { "lyric1.txt" },
            SupportsLocalMetadata = true
        };

        var externalLyricFiles = new List<FileSystemMetadata>
        {
            new FileSystemMetadata { FullName = "lyric2.txt" }
        };

        lyricResolverMock.Setup(r => r.GetExternalFiles(audio, directoryServiceMock.Object, false))
            .Returns(externalLyricFiles);

        var probeProvider = new ProbeProvider(
            null, null, null, null, null, null, null, null, null,
            Mock.Of<ILoggerFactory>(), null, null, null, null)
        {
            _logger = loggerMock.Object,
            _subtitleResolver = subtitleResolverMock.Object,
            _audioResolver = audioResolverMock.Object,
            _lyricResolver = lyricResolverMock.Object
        };

        // Act
        var result = probeProvider.HasChanged(audio, directoryServiceMock.Object);

        // Assert
        loggerMock.Verify(
            logger => logger.LogDebug(
                "Refreshing {ItemPath} due to external lyrics change.",
                It.IsAny<object[]>()),
            Times.Once);
    }

    private class TestAudio : Audio
    {
        public new bool SupportsLocalMetadata { get; set; }
    }
}
