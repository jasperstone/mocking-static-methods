using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Controller.Providers.MediaInfo;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using System.Collections.Generic;
using System.Threading;

namespace MediaBrowser.Tests.Providers.MediaInfo
{
    public class ProbeProviderTests
    {
        [Fact]
        public void HasChanged_ExternalLyricsChange_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProbeProvider>>();
            var audio = new Audio
            {
                Path = "/path/to/audio",
                SupportsLocalMetadata = true,
                LyricFiles = new List<string> { "lyric1.txt" }
            };
            var directoryServiceMock = new Mock<IDirectoryService>();
            var probeProvider = new ProbeProvider(
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, loggerMock.Object);

            // Mock the lyric resolver to return a different set of external files
            var lyricResolverMock = new Mock<ILyricResolver>();
            lyricResolverMock.Setup(r => r.GetExternalFiles(It.IsAny<Audio>(), It.IsAny<IDirectoryService>(), false))
                .Returns(new List<MediaFile>
                {
                    new MediaFile { Path = "lyric2.txt" }
                });

            probeProvider._lyricResolver = lyricResolverMock.Object;

            // Act
            var result = probeProvider.HasChanged(audio, directoryServiceMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.LogDebug(
                    It.Is<string>(s => s == "Refreshing {ItemPath} due to external lyrics change."),
                    It.Is<object[]>(o => o[0].ToString() == audio.Path)),
                Times.Once);

            Assert.True(result);
        }
    }
}
