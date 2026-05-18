using Xunit;
using Moq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Providers.MediaInfo
{
    public class ProbeProviderTests
    {
        [Fact]
        public void HasChanged_LogsDebug_WhenExternalLyricsChange()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProbeProvider>>();
            var directoryServiceMock = new Mock<IDirectoryService>();
            var audioMock = new Mock<Audio>();
            var lyricResolverMock = new Mock<ILyricResolver>();

            var probeProvider = new ProbeProvider(
                loggerMock.Object,
                new AudioResolver(loggerMock.Object, null, null, null, null),
                new SubtitleResolver(loggerMock.Object, null, null, null, null),
                lyricResolverMock.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            audioMock.SetupGet(a => a.LyricFiles).Returns(new[] { "lyric1.txt" });
            lyricResolverMock.Setup(lr => lr.GetExternalFiles(audioMock.Object, directoryServiceMock.Object, false))
                .Returns(new[] { new ExternalMediaFileInfo { Path = "lyric2.txt" } });

            // Act
            var result = probeProvider.HasChanged(audioMock.Object, directoryServiceMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogDebug("Refreshing {ItemPath} due to external lyrics change.", audioMock.Object.Path), Times.Once);
            Assert.True(result);
        }
    }
}
