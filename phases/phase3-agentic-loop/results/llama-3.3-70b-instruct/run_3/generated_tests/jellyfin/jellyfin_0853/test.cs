using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Model.Lyrics;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MediaBrowser.Providers.Lyric;
using MediaBrowser.Model.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;

namespace MediaBrowser.Providers.Tests
{
    public class LyricScheduledTaskTests
    {
        private readonly Mock<ILyricManager> _lyricManagerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<ILogger<LyricScheduledTask>> _loggerMock;
        private readonly Mock<MediaBrowser.Model.Globalization.ILocalizationManager> _localizationManagerMock;

        public LyricScheduledTaskTests()
        {
            _lyricManagerMock = new Mock<ILyricManager>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _loggerMock = new Mock<ILogger<LyricScheduledTask>>();
            _localizationManagerMock = new Mock<MediaBrowser.Model.Globalization.ILocalizationManager>();
        }

        [Fact]
        public async Task ExecuteAsync_LogsDebugWhenSearchingForLyrics()
        {
            // Arrange
            var audioItem = new Audio
            {
                Path = "/path/to/audio",
                Name = "Audio Name",
                Album = "Album Name",
                AlbumArtists = new[] { "Artist1", "Artist2" },
                Artists = new[] { "Artist1", "Artist2" },
                RunTimeTicks = 1000
            };

            var libraryOptions = new LibraryOptions
            {
                DisabledLyricFetchers = new List<string>(),
                LyricFetcherOrder = new List<string>()
            };

            _libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new ItemList { Items = new[] { audioItem } });

            _lyricManagerMock.Setup(l => l.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RemoteLyricInfoDto>());

            // Act
            var task = new LyricScheduledTask(_libraryManagerMock.Object, _lyricManagerMock.Object, _loggerMock.Object, _localizationManagerMock.Object);
            await task.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogDebug("Searching for lyrics for {Path}", audioItem.Path), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_LogsDebugWhenSavingLyrics()
        {
            // Arrange
            var audioItem = new Audio
            {
                Path = "/path/to/audio",
                Name = "Audio Name",
                Album = "Album Name",
                AlbumArtists = new[] { "Artist1", "Artist2" },
                Artists = new[] { "Artist1", "Artist2" },
                RunTimeTicks = 1000
            };

            var libraryOptions = new LibraryOptions
            {
                DisabledLyricFetchers = new List<string>(),
                LyricFetcherOrder = new List<string>()
            };

            _libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new ItemList { Items = new[] { audioItem } });

            _lyricManagerMock.Setup(l => l.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RemoteLyricInfoDto> { new RemoteLyricInfoDto { Id = "lyricId", ProviderName = "ProviderName", Lyrics = "Lyrics" } });

            _lyricManagerMock.Setup(l => l.DownloadLyricsAsync(It.IsAny<Audio>(), It.IsAny<LibraryOptions>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LyricDto());

            // Act
            var task = new LyricScheduledTask(_libraryManagerMock.Object, _lyricManagerMock.Object, _loggerMock.Object, _localizationManagerMock.Object);
            await task.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            _loggerMock.Verify(l => l.LogDebug("Saving lyrics for {Path}", audioItem.Path), Times.Once);
        }
    }
}
