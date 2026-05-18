using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
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
            var audioItem = new Mock<Audio>();
            audioItem.Setup(a => a.GetMediaStreams()).Returns(new List<MediaStream>());

            var libraryOptions = new LibraryOptions();
            var lyricResults = new List<RemoteLyricInfoDto>
            {
                new RemoteLyricInfoDto { Id = "testId", ProviderName = "testProvider", Lyrics = new LyricDto() }
            };

            _libraryManagerMock.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { audioItem.Object });

            _lyricManagerMock.Setup(lm => lm.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lyricResults);

            _libraryManagerMock.Setup(lm => lm.GetLibraryOptions(It.IsAny<Folder>()))
                .Returns(libraryOptions);

            _libraryManagerMock.Setup(lm => lm.RootFolder)
                .Returns(new AggregateFolder { Children = new List<Folder> { new Folder() } });

            _libraryManagerMock.Setup(lm => lm.GetCount(It.IsAny<InternalItemsQuery>()))
                .Returns(1);

            // Act
            await _lyricScheduledTask.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogDebug("Saving lyrics for {Path}", It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var audioItem = new Mock<Audio>();
            audioItem.Setup(a => a.GetMediaStreams()).Returns(new List<MediaStream>());

            _libraryManagerMock.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { audioItem.Object });

            _lyricManagerMock.Setup(lm => lm.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            _libraryManagerMock.Setup(lm => lm.RootFolder)
                .Returns(new AggregateFolder { Children = new List<Folder> { new Folder() } });

            _libraryManagerMock.Setup(lm => lm.GetCount(It.IsAny<InternalItemsQuery>()))
                .Returns(1);

            // Act
            await _lyricScheduledTask.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(It.IsAny<Exception>(), "Error downloading lyrics for {Path}", It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
