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
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Naming.ExternalFiles;

namespace MediaBrowser.Providers.MediaInfo.Tests
{
    public class ProbeProviderTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<ILogger<ProbeProvider>> _loggerMock;
        private readonly Mock<IDirectoryService> _directoryServiceMock;
        private readonly Mock<AudioResolver> _audioResolverMock;
        private readonly Mock<SubtitleResolver> _subtitleResolverMock;
        private readonly Mock<LyricResolver> _lyricResolverMock;
        private readonly ProbeProvider _probeProvider;

        public ProbeProviderTests()
        {
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _loggerMock = new Mock<ILogger<ProbeProvider>>();
            _directoryServiceMock = new Mock<IDirectoryService>();
            _audioResolverMock = new Mock<AudioResolver>(MockBehavior.Strict, _loggerMock.Object, null, null, null, null);
            _subtitleResolverMock = new Mock<SubtitleResolver>(MockBehavior.Strict, _loggerMock.Object, null, null, null, null);
            _lyricResolverMock = new Mock<LyricResolver>(MockBehavior.Strict, _loggerMock.Object, null, null, null, null);

            _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);

            _probeProvider = new ProbeProvider(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                _loggerFactoryMock.Object,
                null,
                null,
                null,
                null);
        }

        [Fact]
        public void HasChanged_ShouldLogDebug_WhenExternalLyricsChange()
        {
            // Arrange
            var audio = new Audio
            {
                SupportsLocalMetadata = true,
                LyricFiles = new List<string> { "lyric1.txt" }
            };

            _lyricResolverMock.Setup(r => r.GetExternalFiles(audio, _directoryServiceMock.Object, false))
                .Returns(new List<ExternalPathParserResult> { new ExternalPathParserResult("lyric2.txt", false, false, false) });

            // Act
            var result = _probeProvider.HasChanged(audio, _directoryServiceMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Refreshing {ItemPath} due to external lyrics change.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            Assert.True(result);
        }
    }
}
