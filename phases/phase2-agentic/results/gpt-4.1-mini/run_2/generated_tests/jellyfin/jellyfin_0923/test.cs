using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
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
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
        private readonly Mock<ISubtitleResolver> _subtitleResolverMock;
        private readonly Mock<IAudioResolver> _audioResolverMock;
        private readonly Mock<ILyricResolver> _lyricResolverMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;

        private class TestProbeProvider : ProbeProvider
        {
            public TestProbeProvider(
                ILogger<ProbeProvider> logger,
                ISubtitleResolver subtitleResolver,
                IAudioResolver audioResolver,
                ILyricResolver lyricResolver)
                : base(
                    null, null, null, null, null, null, null, null, null,
                    new LoggerFactory(), null, null, null, null)
            {
                _logger = logger;
                _subtitleResolver = subtitleResolver;
                _audioResolver = audioResolver;
                _lyricResolver = lyricResolver;
            }

            public new ILogger<ProbeProvider> _logger;
            public new ISubtitleResolver _subtitleResolver;
            public new IAudioResolver _audioResolver;
            public new ILyricResolver _lyricResolver;

            public new bool HasChanged(BaseItem item, IDirectoryService directoryService)
            {
                return base.HasChanged(item, directoryService);
            }
        }

        public ProbeProviderTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<ProbeProvider>>();
            _loggerFactoryMock.Setup(f => f.CreateLogger<ProbeProvider>()).Returns(_loggerMock.Object);

            _subtitleResolverMock = new Mock<ISubtitleResolver>();
            _audioResolverMock = new Mock<IAudioResolver>();
            _lyricResolverMock = new Mock<ILyricResolver>();

            _directoryServiceMock = new Mock<IDirectoryService>();
        }

        [Fact]
        public void HasChanged_LogsDebug_WhenAudioLyricsChanged()
        {
            // Arrange
            var audio = new Audio
            {
                Path = "audioPath",
                SupportsLocalMetadata = true,
                LyricFiles = new List<string> { "lyric1" }
            };

            var externalFiles = new List<MediaBrowser.Controller.Entities.ExternalFileInfo>
            {
                new MediaBrowser.Controller.Entities.ExternalFileInfo { Path = "lyric2" }
            };

            _lyricResolverMock.Setup(l => l.GetExternalFiles(audio, _directoryServiceMock.Object, false))
                .Returns(externalFiles);

            var provider = new TestProbeProvider(_loggerMock.Object, _subtitleResolverMock.Object, _audioResolverMock.Object, _lyricResolverMock.Object);

            // Act
            var result = provider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Refreshing audioPath due to external lyrics change.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Interfaces to mock resolvers for testing
    public interface ISubtitleResolver
    {
        IEnumerable<MediaBrowser.Controller.Entities.ExternalFileInfo> GetExternalFiles(Video video, IDirectoryService directoryService, bool flag);
    }

    public interface IAudioResolver
    {
        IEnumerable<MediaBrowser.Controller.Entities.ExternalFileInfo> GetExternalFiles(Video video, IDirectoryService directoryService, bool flag);
    }

    public interface ILyricResolver
    {
        IEnumerable<MediaBrowser.Controller.Entities.ExternalFileInfo> GetExternalFiles(Audio audio, IDirectoryService directoryService, bool flag);
    }
}
