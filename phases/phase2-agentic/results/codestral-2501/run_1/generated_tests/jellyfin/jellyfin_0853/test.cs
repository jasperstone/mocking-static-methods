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

namespace MediaBrowser.Providers.Lyric.Tests
{
    public class LyricScheduledTaskTests
    {
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<ILyricManager> _lyricManagerMock;
        private readonly Mock<ILogger<LyricScheduledTask>> _loggerMock;
        private readonly Mock<ILocalizationManager> _localizationManagerMock;
        private readonly LyricScheduledTask _lyricScheduledTask;

        public LyricScheduledTaskTests()
        {
            _libraryManagerMock = new Mock<ILibraryManager>();
            _lyricManagerMock = new Mock<ILyricManager>();
            _loggerMock = new Mock<ILogger<LyricScheduledTask>>();
            _localizationManagerMock = new Mock<ILocalizationManager>();

            _lyricScheduledTask = new LyricScheduledTask(
                _libraryManagerMock.Object,
                _lyricManagerMock.Object,
                _loggerMock.Object,
                _localizationManagerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogDebug_WhenLyricsAreFound()
        {
            // Arrange
            var audioItem = new Audio
            {
                Path = "testPath",
                Name = "testName",
                Album = "testAlbum",
                AlbumArtists = new List<string> { "testAlbumArtist" },
                Artists = new List<string> { "testArtist" },
                RunTimeTicks = 1000
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

            _libraryManagerMock.Setup(manager => manager.GetItemList(itemQuery))
                .Returns(new List<BaseItem> { audioItem });

            _lyricManagerMock.Setup(manager => manager.SearchLyricsAsync(
                    It.IsAny<LyricSearchRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RemoteLyricInfoDto> { new RemoteLyricInfoDto { Id = "lyricId" } });

            _lyricManagerMock.Setup(manager => manager.DownloadLyricsAsync(
                    It.IsAny<Audio>(),
                    It.IsAny<LibraryOptions>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LyricDto());

            var progressMock = new Mock<IProgress<double>>();

            // Act
            await _lyricScheduledTask.ExecuteAsync(progressMock.Object, CancellationToken.None);

            // Assert
            _loggerMock.Verify(logger => logger.LogDebug(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }
    }
}
