using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MediaBrowser.Providers.MediaInfo.Tests
{
    public class ProbeProviderTests
    {
        [Fact]
        public void HasChanged_ExternalLyricsChange_LogsDebugMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ProbeProvider>>();
            var mockDirectoryService = new Mock<IDirectoryService>();
            var mockAudioResolver = new Mock<AudioResolver>();
            var mockSubtitleResolver = new Mock<SubtitleResolver>();
            var mockLyricResolver = new Mock<LyricResolver>();

            var audio = new Audio
            {
                Path = "/path/to/audio",
                SupportsLocalMetadata = true,
                LyricFiles = new Collection<string> { "lyric1.srt" }
            };

            var externalLyrics = new List<MediaFile>
            {
                new MediaFile { Path = "lyric1.srt" },
                new MediaFile { Path = "lyric2.srt" } // New external lyric file
            };

            mockLyricResolver.Setup(r => r.GetExternalFiles(audio, It.IsAny<IDirectoryService>(), false))
                .Returns(externalLyrics);

            var probeProvider = new ProbeProvider(
                null, null, null, null, null, null, null, mockLogger.Object, null, null, 
                mockLyricResolver.Object, null, null, null);

            // Act
            var result = probeProvider.HasChanged(audio, mockDirectoryService.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogDebug(
                    "Refreshing {ItemPath} due to external lyrics change.",
                    audio.Path),
                Times.Once);

            Assert.True(result);
        }
    }
}
