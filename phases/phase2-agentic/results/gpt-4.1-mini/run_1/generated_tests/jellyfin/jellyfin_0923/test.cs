using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.IO;
using MediaBrowser.Providers.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.MediaInfo.Tests
{
    public class ProbeProviderTests
    {
        [Fact]
        public void HasChanged_LogsDebug_WhenAudioLyricsChanged()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<ProbeProvider>>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ProbeProvider>()).Returns(loggerMock.Object);

            var audioResolverLoggerMock = new Mock<ILogger<AudioResolver>>();
            var subtitleResolverLoggerMock = new Mock<ILogger<SubtitleResolver>>();
            var lyricResolverLoggerMock = new Mock<ILogger<LyricResolver>>();
            var videoProberLoggerMock = new Mock<ILogger<FFProbeVideoInfo>>();
            var audioProberLoggerMock = new Mock<ILogger<AudioFileProber>>();

            // We need to mock the internal resolvers to control their behavior
            // But since they are private and created inside constructor, we will mock the logger factory to return dummy loggers for them
            loggerFactoryMock.Setup(f => f.CreateLogger<AudioResolver>()).Returns(audioResolverLoggerMock.Object);
            loggerFactoryMock.Setup(f => f.CreateLogger<SubtitleResolver>()).Returns(subtitleResolverLoggerMock.Object);
            loggerFactoryMock.Setup(f => f.CreateLogger<LyricResolver>()).Returns(lyricResolverLoggerMock.Object);
            loggerFactoryMock.Setup(f => f.CreateLogger<FFProbeVideoInfo>()).Returns(videoProberLoggerMock.Object);
            loggerFactoryMock.Setup(f => f.CreateLogger<AudioFileProber>()).Returns(audioProberLoggerMock.Object);

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

            // Create an Audio item with SupportsLocalMetadata = true
            var audioItem = new Audio
            {
                Path = "audioPath",
                SupportsLocalMetadata = true,
                LyricFiles = new List<string> { "lyric1.lrc" }
            };

            // Mock directory service
            var directoryServiceMock = new Mock<IDirectoryService>();

            // We need to mock the _lyricResolver.GetExternalFiles to return different files than audioItem.LyricFiles
            // But _lyricResolver is private, so we cannot mock it directly.
            // Instead, we will create a derived class to override HasChanged for testing or use reflection to set the private field.
            // Since we cannot do that here, we will create a minimal subclass to override HasChanged to simulate the condition.

            // But since the user wants tests for the actual code, we can use a helper class to override the _lyricResolver behavior.

            // Instead, we can create a derived class to override the _lyricResolver.GetExternalFiles method by exposing it.

            // For simplicity, we will create a TestProbeProvider that inherits ProbeProvider and overrides HasChanged to call base.HasChanged but with a mocked _lyricResolver.

            // But since the _lyricResolver is private readonly, we cannot override it easily.

            // Alternative: Use reflection to set _lyricResolver to a mock that returns controlled data.

            var lyricResolverMock = new Mock<LyricResolver>(
                lyricResolverLoggerMock.Object,
                null,
                null,
                null,
                null);

            lyricResolverMock.Setup(lr => lr.GetExternalFiles(audioItem, directoryServiceMock.Object, false))
                .Returns(new List<MediaBrowser.Model.IO.FileSystemMetadata>
                {
                    new MediaBrowser.Model.IO.FileSystemMetadata { Path = "different.lyric" }
                });

            // Use reflection to set private readonly field _lyricResolver
            var lyricResolverField = typeof(ProbeProvider).GetField("_lyricResolver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            lyricResolverField.SetValue(probeProvider, lyricResolverMock.Object);

            // Act
            var result = probeProvider.HasChanged(audioItem, directoryServiceMock.Object);

            // Assert
            Assert.True(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Refreshing audioPath due to external lyrics change.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
