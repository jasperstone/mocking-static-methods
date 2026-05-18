using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Providers.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ProbeProviderTests
{
    [Fact]
    public void LogDebug_Called_WhenExternalLyricsChange()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ProbeProvider>>();
        var directoryServiceMock = new Mock<IDirectoryService>();
        var lyricResolverMock = new Mock<LyricResolver>();
        var audioResolverMock = new Mock<AudioResolver>();
        var subtitleResolverMock = new Mock<SubtitleResolver>();
        var videoProberMock = new Mock<FFProbeVideoInfo>();
        var audioProberMock = new Mock<AudioFileProber>();

        var probeProvider = new ProbeProvider(
            null, null, null, null, null, null, null, null, null,
            Mock.Of<ILoggerFactory>(), null, null, null, null);

        var audio = new Audio
        {
            Path = "audio.mp3",
            LyricFiles = new List<string> { "lyric1.txt" }
        };

        lyricResolverMock.Setup(r => r.GetExternalFiles(audio, directoryServiceMock.Object, false))
            .Returns(new List<FileSystemMetadata> { new FileSystemMetadata { FullName = "lyric2.txt" } });

        // Act
        probeProvider.HasChanged(audio, directoryServiceMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Refreshing {ItemPath} due to external lyrics change.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
