using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using System.Collections.Generic;
using System.Threading;

namespace MediaBrowser.Providers.MediaInfo.Tests
{
    public class ProbeProviderTests
    {
        [Fact]
        public void HasChanged_ExternalLyricsChange_LogsDebugMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProbeProvider>>();
            var directoryServiceMock = new Mock<IDirectoryService>();
            var audioMock = new Mock<Audio>();
            var probeProvider = new ProbeProvider(
                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, loggerMock.Object);

            audioMock.SetupGet(a => a.SupportsLocalMetadata).Returns(true);
            audioMock.SetupGet(a => a.LyricFiles).Returns(new List<string> { "lyric1.txt" });

            var externalFiles = new HashSet<string> { "lyric2.txt" };
            var lyricResolverMock = new Mock<ILyricResolver>();
            lyricResolverMock.Setup(r => r.GetExternalFiles(It.IsAny<Audio>(), It.IsAny<IDirectoryService>(), false))
                .Returns(new List<MediaFile> { new MediaFile { Path = "lyric2.txt" } });

            probeProvider._lyricResolver = lyricResolverMock.Object;

            // Act
            var result = probeProvider.HasChanged(audioMock.Object, directoryServiceMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.LogDebug("Refreshing {ItemPath} due to external lyrics change.", audioMock.Object.Path),
                Times.Once);

            Assert.True(result);
        }
    }
}
