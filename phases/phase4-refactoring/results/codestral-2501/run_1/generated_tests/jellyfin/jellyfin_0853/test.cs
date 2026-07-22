using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Lyrics;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Providers.Lyric.Tests
{
    public class LyricScheduledTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsDebugForEachAudioItem()
        {
            // Arrange
            var libraryManagerMock = new Mock<ILibraryManager>();
            var lyricManagerMock = new Mock<ILyricManager>();
            var loggerMock = new Mock<ILogger<LyricScheduledTask>>();
            var localizationManagerMock = new Mock<ILocalizationManager>();

            var audioItem = new Audio
            {
                Path = "path/to/audio",
                Name = "SongName",
                Album = "AlbumName",
                AlbumArtists = new[] { "AlbumArtist" },
                Artists = new[] { "Artist" },
                RunTimeTicks = 1000000
            };

            var libraryOptions = new LibraryOptions
            {
                DisabledLyricFetchers = new List<string>(),
                LyricFetcherOrder = new List<string>()
            };

            var itemQuery = new InternalItemsQuery
            {
                Recursive = true,
                IsVirtualItem = false,
                IncludeItemTypes = new[] { BaseItemKind.Audio },
                DtoOptions = new DtoOptions(false),
                MediaTypes = new[] { MediaType.Audio },
                SourceTypes = new[] { SourceType.Library },
                Limit = 100,
                Parent = new Folder()
            };

            libraryManagerMock.Setup(lm => lm.GetItemList(itemQuery)).Returns(new List<BaseItem> { audioItem });
            libraryManagerMock.Setup(lm => lm.GetLibraryOptions(It.IsAny<Folder>())).Returns(libraryOptions);
            libraryManagerMock.Setup(lm => lm.RootFolder.Children).Returns(new List<Folder> { new Folder() });

            lyricManagerMock.Setup(lm => lm.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RemoteLyricInfoDto> { new RemoteLyricInfoDto { Id = "lyricId" } });

            var task = new LyricScheduledTask(
                libraryManagerMock.Object,
                lyricManagerMock.Object,
                loggerMock.Object,
                localizationManagerMock.Object);

            var progressMock = new Mock<IProgress<double>>();

            // Act
            await task.ExecuteAsync(progressMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                logger => logger.LogDebug("Searching for lyrics for {Path}", It.IsAny<object[]>()),
                Times.Once);

            loggerMock.Verify(
                logger => logger.LogDebug("Saving lyrics for {Path}", It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
