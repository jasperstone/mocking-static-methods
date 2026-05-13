using Xunit;
using Moq;
using MediaBrowser.Providers.MediaInfo;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Providers.Tests
{
    public class ProbeProviderTests
    {
        [Fact]
        public void HasChanged_LogsDebugMessage_WhenExternalLyricsChange()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProbeProvider>>();
            var probeProvider = new ProbeProvider(
                loggerMock.Object,
                new AudioResolver(new Mock<ILogger<AudioResolver>>().Object, null, null, null, null),
                new SubtitleResolver(new Mock<ILogger<SubtitleResolver>>().Object, null, null, null, null),
                new LyricResolver(new Mock<ILogger<LyricResolver>>().Object, null, null, null, null),
                new FFProbeVideoInfo(new Mock<ILogger<FFProbeVideoInfo>>().Object, null, null, null, null, null, null, null, null, null),
                new AudioFileProber(new Mock<ILogger<AudioFileProber>>().Object, null, null, null, null, null, null, null));

            var audio = new Audio
            {
                Path = "path",
                SupportsLocalMetadata = true,
                LyricFiles = new List<string> { "lyric1" }
            };

            var directoryServiceMock = new Mock<IDirectoryService>();
            directoryServiceMock.Setup(ds => ds.GetFile(audio.Path)).Returns(new FileInfo("path"));

            var lyricResolverMock = new Mock<ILyricResolver>();
            lyricResolverMock.Setup(lr => lr.GetExternalFiles(audio, directoryServiceMock.Object, false)).Returns(new List<ExternalFile> { new ExternalFile { Path = "lyric2" } });

            // Act
            var result = probeProvider.HasChanged(audio, directoryServiceMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Refreshing {ItemPath} due to external lyrics change.", audio.Path), Times.Once);
        }
    }
}
