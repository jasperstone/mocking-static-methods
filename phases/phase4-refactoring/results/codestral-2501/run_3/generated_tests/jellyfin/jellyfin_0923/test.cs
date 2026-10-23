using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Providers.MediaInfo;
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
            var audioResolverMock = new Mock<AudioResolver>(MockBehavior.Strict, null, null, null, null, null);
            var subtitleResolverMock = new Mock<SubtitleResolver>(MockBehavior.Strict, null, null, null, null, null);
            var lyricResolverMock = new Mock<LyricResolver>(MockBehavior.Strict, null, null, null, null, null);

            var probeProvider = new ProbeProvider(
                null, null, null, null, null, null, null, null, null,
                Mock.Of<ILoggerFactory>(), null, null, null, null);

            var audio = new Audio
            {
                SupportsLocalMetadata = true,
                LyricFiles = new List<string> { "lyric1.txt" }
            };

            var externalFiles = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { FullName = "lyric2.txt" }
            };

            lyricResolverMock.Setup(r => r.GetExternalFiles(audio, directoryServiceMock.Object, false))
                .Returns(externalFiles);

            // Act
            var result = probeProvider.HasChanged(audio, directoryServiceMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Refreshing")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
