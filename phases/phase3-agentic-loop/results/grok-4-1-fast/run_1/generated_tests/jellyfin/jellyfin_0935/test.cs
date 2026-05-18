using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Trickplay.Tests
{
    public class TrickplayImagesTaskTests
    {
        private readonly Mock<ILogger<TrickplayImagesTask>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<ILocalizationManager> _localizationManagerMock;
        private readonly Mock<ITrickplayManager> _trickplayManagerMock;
        private readonly TrickplayImagesTask _task;

        public TrickplayImagesTaskTests()
        {
            _loggerMock = new Mock<ILogger<TrickplayImagesTask>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _localizationManagerMock = new Mock<ILocalizationManager>();
            _trickplayManagerMock = new Mock<ITrickplayManager>();

            _task = new TrickplayImagesTask(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _localizationManagerMock.Object,
                _trickplayManagerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_LogsError_WhenRefreshTrickplayDataThrowsException()
        {
            // Arrange
            var video = new Video { Name = "Test Video" };
            var query = new InternalItemsQuery
            {
                MediaTypes = new[] { MediaBrowser.Model.Entities.MediaType.Video },
                SourceTypes = new[] { MediaBrowser.Model.Entities.SourceType.Library },
                IsVirtualItem = false,
                IsFolder = false,
                Recursive = true,
                IncludeOwnedItems = true,
                Limit = 100
            };

            _libraryManagerMock.Setup(m => m.GetCount(query)).Returns(1);
            _libraryManagerMock.SetupSequence(m => m.GetItemList(query))
                .Returns(new[] { video }.Cast<BaseItem>())
                .Returns(new BaseItem[0]);

            _libraryManagerMock.Setup(m => m.GetLibraryOptions(video))
                .Returns(new LibraryOptions());

            _trickplayManagerMock.Setup(m => m.RefreshTrickplayDataAsync(video, false, It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            var progress = new Mock<IProgress<double>>().Object;
            var cancellationToken = new CancellationToken();

            // Act
            await _task.ExecuteAsync(progress, cancellationToken);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => CheckLogMessage(v, "Test Video")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static bool CheckLogMessage<TState>(TState state, Type type)
        {
            return state?.ToString()?.Contains("Error creating trickplay files for {ItemName}") == true ||
                   state?.ToString()?.Contains("Test Video") == true;
        }

        [Fact]
        public async Task ExecuteAsync_ProcessesAllVideos_WithoutExceptions()
        {
            // Arrange
            var video1 = new Video { Name = "Video 1" };
            var video2 = new Video { Name = "Video 2" };
            var query = new InternalItemsQuery
            {
                MediaTypes = new[] { MediaBrowser.Model.Entities.MediaType.Video },
                SourceTypes = new[] { MediaBrowser.Model.Entities.SourceType.Library },
                IsVirtualItem = false,
                IsFolder = false,
                Recursive = true,
                IncludeOwnedItems = true,
                Limit = 100
            };

            _libraryManagerMock.Setup(m => m.GetCount(query)).Returns(2);
            _libraryManagerMock.SetupSequence(m => m.GetItemList(query))
                .Returns(new[] { video1, video2 }.Cast<BaseItem>())
                .Returns(new BaseItem[0]);

            _libraryManagerMock.Setup(m => m.GetLibraryOptions(It.IsAny<Video>()))
                .Returns(new LibraryOptions());

            _trickplayManagerMock.Setup(m => m.RefreshTrickplayDataAsync(It.IsAny<Video>(), false, It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var progress = new Mock<IProgress<double>>().Object;
            var cancellationToken = new CancellationToken();

            // Act
            await _task.ExecuteAsync(progress, cancellationToken);

            // Assert
            _trickplayManagerMock.Verify(m => m.RefreshTrickplayDataAsync(video1, false, It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _trickplayManagerMock.Verify(m => m.RefreshTrickplayDataAsync(video2, false, It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _loggerMock.Verify(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }
    }
}
