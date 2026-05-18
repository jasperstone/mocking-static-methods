using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Lyrics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Tests.Providers.Lyric
{
    public class LyricScheduledTaskTests
    {
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<ILyricManager> _lyricManagerMock;
        private readonly Mock<ILogger<MediaBrowser.Providers.Lyric.LyricScheduledTask>> _loggerMock;
        private readonly Mock<ILocalizationManager> _localizationManagerMock;

        public LyricScheduledTaskTests()
        {
            _libraryManagerMock = new Mock<ILibraryManager>();
            _lyricManagerMock = new Mock<ILyricManager>();
            _loggerMock = new Mock<ILogger<MediaBrowser.Providers.Lyric.LyricScheduledTask>>();
            _localizationManagerMock = new Mock<ILocalizationManager>();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogDebug_WhenSearchingForLyrics()
        {
            // Arrange
            var task = new MediaBrowser.Providers.Lyric.LyricScheduledTask(
                _libraryManagerMock.Object,
                _lyricManagerMock.Object,
                _loggerMock.Object,
                _localizationManagerMock.Object);

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
            var getMediaStreamsMethod = new Func<IEnumerable<MediaStream>>(() => mediaStreams);
            // Use reflection or a derived class to set GetMediaStreams if needed, or assume it's virtual and can be mocked.

            // Setup library manager to return the audio item
            var itemList = new List<Audio> { audioItem };
            _libraryManagerMock.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(itemList);

            // Setup lyric manager to return a lyric result
            var lyricResults = new List<LyricsResult> { new LyricsResult { Id = "lyricId" } };
            _lyricManagerMock.Setup(m => m.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lyricResults);

            // Setup localization manager
            _localizationManagerMock.Setup(m => m.GetLocalizedString(It.IsAny<string>()))
                .Returns("LocalizedString");

            // Act
            await task.ExecuteAsync(progress.Object, cts.Token);

            // Assert
            _loggerMock.Verify(
                m => m.LogDebug(It.Is<string>(s => s.Contains("Searching for lyrics for")), It.IsAny<object[]>()),
                Times.AtLeastOnce);
        }
    }
}
