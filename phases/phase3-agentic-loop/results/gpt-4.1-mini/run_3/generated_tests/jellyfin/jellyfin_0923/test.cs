using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Providers.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.MediaInfo.Tests
{
    public class ProbeProviderTests
    {
        private class DummyExternalFile
        {
            public string Path { get; set; }
        }

        [Fact]
        public void HasChanged_LogsDebug_WhenExternalLyricsChange()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<ProbeProvider>>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ProbeProvider>()).Returns(loggerMock.Object);

            var audioResolverLogger = new Mock<ILogger<AudioResolver>>();
            var subtitleResolverLogger = new Mock<ILogger<SubtitleResolver>>();
            var lyricResolverLogger = new Mock<ILogger<LyricResolver>>();
            var videoProberLogger = new Mock<ILogger<FFProbeVideoInfo>>();
            var audioProberLogger = new Mock<ILogger<AudioFileProber>>();

            loggerFactoryMock.Setup(f => f.CreateLogger<AudioResolver>()).Returns(audioResolverLogger.Object);
            loggerFactoryMock.Setup(f => f.CreateLogger<SubtitleResolver>()).Returns(subtitleResolverLogger.Object);
            loggerFactoryMock.Setup(f => f.CreateLogger<LyricResolver>()).Returns(lyricResolverLogger.Object);
            loggerFactoryMock.Setup(f => f.CreateLogger<FFProbeVideoInfo>()).Returns(videoProberLogger.Object);
            loggerFactoryMock.Setup(f => f.CreateLogger<AudioFileProber>()).Returns(audioProberLogger.Object);

            var probeProvider = new ProbeProvider(
                mediaSourceManager: null,
                mediaEncoder: null,
                blurayExaminer: null,
                localization: null,
                chapterManager: null,
                config: null,
                subtitleManager: null,
                libraryManager: null,
                fileSystem: null,
                loggerFactory: loggerFactoryMock.Object,
                namingOptions: null,
                lyricManager: null,
                mediaAttachmentRepository: null,
                mediaStreamRepository: null);

            var audioMock = new Mock<Audio>();
            audioMock.Setup(a => a.SupportsLocalMetadata).Returns(true);
            audioMock.Setup(a => a.LyricFiles).Returns(new List<string> { "lyric1.lrc" });
            audioMock.Setup(a => a.Path).Returns("audioPath");

            var directoryServiceMock = new Mock<IDirectoryService>();

            // Setup LyricResolver to return different external files than audio.LyricFiles to trigger the log
            var lyricResolverMock = new Mock<LyricResolver>(lyricResolverLogger.Object, null, null, null, null);
            lyricResolverMock.Setup(lr => lr.GetExternalFiles(audioMock.Object, directoryServiceMock.Object, false))
                .Returns(new List<DummyExternalFile>
                {
                    new DummyExternalFile { Path = "different.lyrics" }
                }.Select(x => new { Path = x.Path }));

            // Replace the private _lyricResolver field with our mock using reflection
            var lyricResolverField = typeof(ProbeProvider).GetField("_lyricResolver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            lyricResolverField.SetValue(probeProvider, lyricResolverMock.Object);

            // Act
            var result = probeProvider.HasChanged(audioMock.Object, directoryServiceMock.Object);

            // Assert
            Assert.True(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Refreshing") && v.ToString().Contains("audioPath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }
    }
}
