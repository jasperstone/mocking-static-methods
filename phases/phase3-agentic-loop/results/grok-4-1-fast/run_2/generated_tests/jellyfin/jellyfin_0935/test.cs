using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace MediaBrowser.Providers.Trickplay.Tests;

public class TrickplayImagesTaskTests
{
    private readonly Mock<ILogger<TrickplayImagesTask>> _loggerMock;
    private readonly Mock<ILibraryManager> _libraryManagerMock;
    private readonly Mock<ILocalizationManager> _localizationMock;
    private readonly Mock<ITrickplayManager> _trickplayManagerMock;
    private readonly TrickplayImagesTask _task;

    public TrickplayImagesTaskTests()
    {
        _loggerMock = new Mock<ILogger<TrickplayImagesTask>>();
        _libraryManagerMock = new Mock<ILibraryManager>();
        _localizationMock = new Mock<ILocalizationManager>();
        _trickplayManagerMock = new Mock<ITrickplayManager>();

        _task = new TrickplayImagesTask(
            _loggerMock.Object,
            _libraryManagerMock.Object,
            _localizationMock.Object,
            _trickplayManagerMock.Object);
    }

    [Fact]
    public void Name_Get_ReturnsLocalizedString()
    {
        _localizationMock.Setup(x => x.GetLocalizedString("TaskRefreshTrickplayImages"))
            .Returns("Refresh Trickplay Images");

        var result = _task.Name;

        Assert.Equal("Refresh Trickplay Images", result);
    }

    [Fact]
    public void Key_Get_ReturnsRefreshTrickplayImages()
    {
        var result = _task.Key;

        Assert.Equal("RefreshTrickplayImages", result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRefreshTrickplayDataThrowsException_LogsErrorWithItemName()
    {
        // Arrange
        var video = new Video { Name = "Test Video" };
        var query = new InternalItemsQuery
        {
            MediaTypes = new[] { MediaType.Video },
            SourceTypes = new[] { SourceType.Library },
            IsVirtualItem = false,
            IsFolder = false,
            Recursive = true,
            IncludeOwnedItems = true,
            Limit = 100
        };

        _libraryManagerMock.Setup(x => x.GetCount(query)).Returns(1);
        _libraryManagerMock.SetupSequence(x => x.GetItemList(It.IsAny<InternalItemsQuery>()))
            .Returns(new[] { video }.Cast<BaseItem>().ToList().AsReadOnly());
        _libraryManagerMock.Setup(x => x.GetLibraryOptions(video))
            .Returns(new LibraryOptions());

        _trickplayManagerMock.Setup(x => x.RefreshTrickplayDataAsync(video, false, It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        var progress = new Progress<double>();
        var cancellationToken = new CancellationToken();

        // Act
        await _task.ExecuteAsync(progress, cancellationToken);

        // Assert - verify the extension method was called
        _loggerMock.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoVideosExists_CompletesWithoutError()
    {
        // Arrange
        var query = new InternalItemsQuery
        {
            MediaTypes = new[] { MediaType.Video },
            SourceTypes = new[] { SourceType.Library },
            IsVirtualItem = false,
            IsFolder = false,
            Recursive = true,
            IncludeOwnedItems = true,
            Limit = 100
        };

        _libraryManagerMock.Setup(x => x.GetCount(query)).Returns(0);

        var progress = new Progress<double>();
        var cancellationToken = new CancellationToken();

        // Act
        await _task.ExecuteAsync(progress, cancellationToken);

        // Assert
        _trickplayManagerMock.Verify(x => x.RefreshTrickplayDataAsync(It.IsAny<Video>(), It.IsAny<bool>(), It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
