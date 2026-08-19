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
        var audioItem = new Audio { Path = "/music/song.mp3", Name = "Song" };
        
        // Mock media streams - no lyrics
        var mockStreams = new Mock<IReadOnlyList<MediaStream>>();
        mockStreams.Setup(s => s.All(It.IsAny<Func<MediaStream, bool>>())).Returns(true);
        Mock.Get(audioItem).Setup(a => a.GetMediaStreams()).Returns(mockStreams.Object);

        _mockLibraryManager.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
        
        var mockRootFolder = new Mock<AggregateFolder>();
        mockRootFolder.Setup(f => f.Children).Returns(new List<BaseItem>());
        _mockLibraryManager.Setup(m => m.RootFolder).Returns(mockRootFolder.Object);
        
        var mockLibrary = new Mock<AggregateFolder>().Object;
        _mockLibraryManager.SetupSequence(m => m.RootFolder.Children)
            .Returns(new[] { mockLibrary });
            
        _mockLibraryManager.Setup(m => m.GetLibraryOptions(mockLibrary))
            .Returns(new Mock<LibraryOptions>().Object);
        
        _mockLibraryManager.SetupSequence(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
            .Returns(new BaseItem[] { audioItem })
            .Returns(Array.Empty<BaseItem>());

        var mockLyricResult = new Mock<RemoteLyricInfoDto>().Object;
        Mock.Get(mockLyricResult).SetupGet(x => x.Id).Returns("lyric1");
        _mockLyricManager.Setup(m => m.SearchLyricsAsync(It.IsAny<LyricSearchRequest>(), cancellationToken))
            .ReturnsAsync(new List<RemoteLyricInfoDto> { mockLyricResult });

        _mockLyricManager.Setup(m => m.DownloadLyricsAsync(
            It.IsAny<Audio>(), 
            It.IsAny<LibraryOptions>(), 
            "lyric1", 
            cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _task.ExecuteAsync(progress, cancellationToken);

        // Assert - Verify the specific LogDebug call on line 132
        _mockLogger.Verify(
            l => l.LogDebug("Saving lyrics for {Path}", "/music/song.mp3"),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_LogsSearchingLyrics_WhenNoLyricStreams()
    {
        // Arrange
        var progress = new Progress<double>();
        var cancellationToken = new CancellationToken();
        var audioItem = new Audio { Path = "/music/song.mp3", Name = "Song" };
        
        // Mock media streams - no lyrics
        var mockStreams = new Mock<IReadOnlyList<MediaStream>>();
        mockStreams.Setup(s => s.All(It.IsAny<Func<MediaStream, bool>>())).Returns(true);
        Mock.Get(audioItem).Setup(a => a.GetMediaStreams()).Returns(mockStreams.Object);

        _mockLibraryManager.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
        
        var mockRootFolder = new Mock<AggregateFolder>();
        mockRootFolder.Setup(f => f.Children).Returns(new List<BaseItem>());
        _mockLibraryManager.Setup(m => m.RootFolder).Returns(mockRootFolder.Object);
        
        var mockLibrary = new Mock<AggregateFolder>().Object;
        _mockLibraryManager.SetupSequence(m => m.RootFolder.Children)
            .Returns(new[] { mockLibrary });
            
        _mockLibraryManager.Setup(m => m.GetLibraryOptions(mockLibrary))
            .Returns(new Mock<LibraryOptions>().Object);
            
        _mockLibraryManager.SetupSequence(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
            .Returns(new BaseItem[] { audioItem })
            .Returns(Array.Empty<BaseItem>());

        // Act
        await _task.ExecuteAsync(progress, cancellationToken);

        // Assert
        _mockLogger.Verify(
            l => l.LogDebug("Searching for lyrics for {Path}", "/music/song.mp3"),
            Times.Once);
    }
}
