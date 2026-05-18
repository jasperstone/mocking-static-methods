using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Lyrics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests
{
    public class LyricScheduledTaskTests
    {
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<ILyricManager> _lyricManagerMock;
        private readonly Mock<ILogger<LyricScheduledTask>> _loggerMock;
        private readonly Mock<ILocalizationManager> _localizationManagerMock;
        private readonly LyricScheduledTask _task;

        public LyricScheduledTaskTests()
        {
            _libraryManagerMock = new Mock<ILibraryManager>();
            _lyricManagerMock = new Mock<ILyricManager>();
            _loggerMock = new Mock<ILogger<LyricScheduledTask>>();
            _localizationManagerMock = new Mock<ILocalizationManager>();

            _localizationManagerMock.Setup(l => l.GetLocalizedString(It.IsAny<string>()))
                .Returns<string>(s => s);

            _task = new LyricScheduledTask(
                _libraryManagerMock.Object,
                _lyricManagerMock.Object,
                _loggerMock.Object,
                _localizationManagerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_Should_LogDebug_When_SearchingForLyrics()
        {
            // Arrange
            var progress = new Mock<IProgress<double>>();
            var cts = new CancellationTokenSource();

            var audioItem = new Audio
            {
                Path = "path/to/audio",
                Name = "Song",
                Album = "Album",
                Artists = new[] { "Artist" },
                AlbumArtists = new[] { "AlbumArtist" },
                RunTimeTicks = 123456
            };

            var mediaStream = new MediaStream { Type = MediaStreamType.Audio };
            var mediaStreams = new List<MediaStream> { mediaStream };
            var mediaStreamsFunc = new Func<IEnumerable<MediaStream>>(() => mediaStreams);
            var getMediaStreamsMethod = typeof(Audio).GetMethod("GetMediaStreams");
            var getMediaStreamsDelegate = (Func<IEnumerable<MediaStream>>)Delegate.CreateDelegate(typeof(Func<IEnumerable<MediaStream>>), audioItem, getMediaStreamsMethod);

            // Setup library manager to return one audio item
            var itemQuery = new InternalItemsQuery { StartIndex = 0, Limit = 1, Parent = null };
            _libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<Audio> { audioItem });

            // Setup lyric manager to return a lyric result
            var lyricResult = new LyricResult { Id = "lyricId" };
            _lyricManagerMock.Setup(l => l.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<LyricResult> { lyricResult });
            _lyricManagerMock.Setup(l => l.DownloadLyricsAsync(It.IsAny<Audio>(), It.IsAny<DtoOptions>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _task.ExecuteAsync(progress.Object, cts.Token);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Searching for lyrics for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Saving lyrics for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
