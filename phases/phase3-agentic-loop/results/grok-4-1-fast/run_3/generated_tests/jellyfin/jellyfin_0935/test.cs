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
using MediaBrowser.Providers.Trickplay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Trickplay.Tests
{
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

            _localizationMock
                .Setup(l => l.GetLocalizedString(It.IsAny<string>()))
                .Returns("LocalizedString");

            _task = new TrickplayImagesTask(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _localizationMock.Object,
                _trickplayManagerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_LogsError_WhenRefreshTrickplayDataAsyncThrows()
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

            var video = new Video { Name = "Test Video", Id = Guid.NewGuid() };
            _libraryManagerMock.Setup(m => m.GetCount(query)).Returns(1);
            _libraryManagerMock.Setup(m => m.GetItemList(query)).Returns(new[] { video });
            _libraryManagerMock.Setup(m => m.GetLibraryOptions(video)).Returns(new LibraryOptions());

            _trickplayManagerMock
                .Setup(m => m.RefreshTrickplayDataAsync(video, false, It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            var progress = new Mock<IProgress<double>>().Object;
            var cancellationToken = new CancellationToken();

            // Act
            var executeTask = _task.ExecuteAsync(progress, cancellationToken);
            await executeTask;

            // Assert
            _trickplayManagerMock.Verify(
                m => m.RefreshTrickplayDataAsync(
                    video,
                    false,
                    It.IsAny<LibraryOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ContainsMessage(v, "Error creating trickplay files for Test Video")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_HandlesMultipleVideosWithOneError_LogsErrorOnce()
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

            var video1 = new Video { Name = "Video 1", Id = Guid.NewGuid() };
            var video2 = new Video { Name = "Video 2", Id = Guid.NewGuid() };
            _libraryManagerMock.Setup(m => m.GetCount(query)).Returns(2);
            _libraryManagerMock.Setup(m => m.GetItemList(query)).Returns(new[] { video1, video2 });
            _libraryManagerMock.Setup(m => m.GetLibraryOptions(It.IsAny<Video>())).Returns(new LibraryOptions());

            _trickplayManagerMock
                .Setup(m => m.RefreshTrickplayDataAsync(video1, false, It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Error on video1"));

            _trickplayManagerMock
                .Setup(m => m.RefreshTrickplayDataAsync(video2, false, It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var progress = new Mock<IProgress<double>>().Object;
            var cancellationToken = new CancellationToken();

            // Act
            var executeTask = _task.ExecuteAsync(progress, cancellationToken);
            await executeTask;

            // Assert
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ContainsMessage(v, "Error creating trickplay files for Video 1")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static bool ContainsMessage<TState>(TState state, string expectedMessage)
        {
            return state?.ToString()?.Contains(expectedMessage, StringComparison.Ordinal) == true;
        }
    }
}
