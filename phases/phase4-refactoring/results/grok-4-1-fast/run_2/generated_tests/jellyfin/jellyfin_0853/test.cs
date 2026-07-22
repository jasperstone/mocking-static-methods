using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Lyrics;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Lyrics;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Lyric.Tests;

public class LyricScheduledTaskTests
{
    private readonly Mock<ILibraryManager> _mockLibraryManager;
    private readonly Mock<ILyricManager> _mockLyricManager;
    private readonly Mock<ILogger<LyricScheduledTask>> _mockLogger;
    private readonly Mock<ILocalizationManager> _mockLocalizationManager;
    private readonly LyricScheduledTask _task;

    public LyricScheduledTaskTests()
    {
        _mockLibraryManager = new Mock<ILibraryManager>();
        _mockLyricManager = new Mock<ILyricManager>();
        _mockLogger = new Mock<ILogger<LyricScheduledTask>>();
        _mockLocalizationManager = new Mock<ILocalizationManager>();

        _task = new LyricScheduledTask(
            _mockLibraryManager.Object,
            _mockLyricManager.Object,
            _mockLogger.Object,
            _mockLocalizationManager.Object);
    }

    [Fact]
    public async Task ExecuteAsync_LogsSavingLyrics_WhenLyricResultsFound()
    {
        // Arrange
        var progress = new Progress<double>();
        var cancellationToken = new CancellationToken();
        var fakePath = "/path/to/song.mp3";

        SetupLibraryManagerWithAudioItem(fakePath);
        SetupLyricManagerWithResults(fakePath);

        // Act
        await _task.ExecuteAsync(progress, cancellationToken);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Saving lyrics for {Path}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private void SetupLibraryManagerWithAudioItem(string fakePath)
    {
        var dtoOptions = new DtoOptions(false);
        var audioItem = new Mock<Audio>();
        audioItem.Setup(x => x.Path).Returns(fakePath);
        audioItem.Setup(x => x.Name).Returns("Test Song");
        audioItem.Setup(x => x.GetMediaStreams()).Returns(new List<MediaStream>());
        audioItem.Setup(x => x.Album).Returns("Test Album");
        audioItem.Setup(x => x.AlbumArtists).Returns(new[] { "Artist1" });
        audioItem.Setup(x => x.Artists).Returns(new[] { "Artist1" });
        audioItem.Setup(x => x.RunTimeTicks).Returns(TimeSpan.FromMinutes(3).Ticks);

        var library = new Mock<AggregateFolder>();
        var rootFolder = new Mock<AggregateFolder>();
        rootFolder.Setup(x => x.Children).Returns(new[] { library.Object });

        _mockLibraryManager.Setup(x => x.RootFolder).Returns(rootFolder.Object);
        _mockLibraryManager.Setup(x => x.GetLibraryOptions(library.Object))
            .Returns(new Mock<LibraryOptions>().Object);

        _mockLibraryManager.Setup(x => x.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
        _mockLibraryManager.Setup(x => x.GetItemList(It.IsAny<InternalItemsQuery>()))
            .Returns(new BaseItem[] { audioItem.Object });
    }

    private void SetupLyricManagerWithResults(string fakePath)
    {
        var lyricResult = new RemoteLyricInfoDto 
        { 
            Id = "123",
            ProviderName = "TestProvider",
            Lyrics = new LyricDto()
        };
        _mockLyricManager.Setup(x => x.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RemoteLyricInfoDto> { lyricResult });

        _mockLyricManager.Setup(x => x.DownloadLyricsAsync(
            It.IsAny<Audio>(),
            It.IsAny<LibraryOptions>(),
            "123",
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
