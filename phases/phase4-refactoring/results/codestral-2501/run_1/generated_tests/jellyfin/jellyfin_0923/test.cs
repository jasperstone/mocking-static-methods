using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.MediaInfo.Tests
{
    public class ProbeProviderTests
    {
        [Fact]
        public void HasChanged_ShouldLogDebug_WhenExternalLyricsChange()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProbeProvider>>();
            var directoryServiceMock = new Mock<IDirectoryService>();
            var subtitleResolverMock = new Mock<SubtitleResolver>();
            var audioResolverMock = new Mock<AudioResolver>();
            var lyricResolverMock = new Mock<LyricResolver>();

            var audio = new Mock<Audio>();
            audio.Setup(a => a.LyricFiles).Returns(new List<string> { "lyric1.txt" });
            audio.Setup(a => a.SupportsLocalMetadata).Returns(true);

            var externalFiles = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { FullName = "lyric2.txt" }
            };

            lyricResolverMock.Setup(r => r.GetExternalFiles(audio.Object, directoryServiceMock.Object, false))
                .Returns(externalFiles);

            var probeProvider = new ProbeProvider(
                null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                loggerMock.Object, subtitleResolverMock.Object, audioResolverMock.Object, lyricResolverMock.Object);

            // Act
            var result = probeProvider.HasChanged(audio.Object, directoryServiceMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogDebug("Refreshing {ItemPath} due to external lyrics change.", audio.Object.Path),
                Times.Once);
            Assert.True(result);
        }

        [Fact]
        public void HasChanged_ShouldNotLogDebug_WhenNoExternalLyricsChange()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProbeProvider>>();
            var directoryServiceMock = new Mock<IDirectoryService>();
            var subtitleResolverMock = new Mock<SubtitleResolver>();
            var audioResolverMock = new Mock<AudioResolver>();
            var lyricResolverMock = new Mock<LyricResolver>();

            var audio = new Mock<Audio>();
            audio.Setup(a => a.LyricFiles).Returns(new List<string> { "lyric1.txt" });
            audio.Setup(a => a.SupportsLocalMetadata).Returns(true);

            var externalFiles = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { FullName = "lyric1.txt" }
            };

            lyricResolverMock.Setup(r => r.GetExternalFiles(audio.Object, directoryServiceMock.Object, false))
                .Returns(externalFiles);

            var probeProvider = new ProbeProvider(
                null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                loggerMock.Object, subtitleResolverMock.Object, audioResolverMock.Object, lyricResolverMock.Object);

            // Act
            var result = probeProvider.HasChanged(audio.Object, directoryServiceMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogDebug("Refreshing {ItemPath} due to external lyrics change.", audio.Object.Path),
                Times.Never);
            Assert.False(result);
        }
    }
}
