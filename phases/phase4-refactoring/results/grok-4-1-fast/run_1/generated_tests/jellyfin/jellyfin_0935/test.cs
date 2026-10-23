using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Entities;
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
        private readonly Mock<ITrickplayManager> _trickplayManagerMock;
        private readonly Mock<ILocalizationManager> _localizationMock;
        private readonly TrickplayImagesTask _task;

        public TrickplayImagesTaskTests()
        {
            _loggerMock = new Mock<ILogger<TrickplayImagesTask>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _trickplayManagerMock = new Mock<ITrickplayManager>();
            _localizationMock = new Mock<ILocalizationManager>();

            _task = new TrickplayImagesTask(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _localizationMock.Object,
                _trickplayManagerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenTrickplayRefreshThrowsException_LogsErrorWithItemName()
        {
            // Arrange
            var video = new Video { Name = "Test Video", Id = Guid.NewGuid() };
            var query = CreateVideoQuery();
            
            _libraryManagerMock.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            _libraryManagerMock.SetupSequence(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new[] { video });
            _libraryManagerMock.Setup(m => m.GetLibraryOptions(video)).Returns(new Dictionary<string, string>());
            _trickplayManagerMock.Setup(m => m.RefreshTrickplayDataAsync(video, false, It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(new InvalidOperationException("Test exception"));

            var progress = new Progress<double>();
            var cts = new CancellationTokenSource();

            // Act
            await _task.ExecuteAsync(progress, cts.Token);

            // Assert
            _trickplayManagerMock.Verify(m => m.RefreshTrickplayDataAsync(video, false, It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
            _loggerMock.Verify(
                m => m.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>(state => state.ToString().Contains("Error creating trickplay files for Test Video")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipleVideos_ProcessesAllAndReportsProgress()
        {
            // Arrange
            var video1 = new Video { Name = "Video 1", Id = Guid.NewGuid() };
            var video2 = new Video { Name = "Video 2", Id = Guid.NewGuid() };
            var progressValues = new List<double>();
            var progress = new Progress<double>(p => progressValues.Add(p));
            
            _libraryManagerMock.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(2);
            _libraryManagerMock.SetupSequence(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new[] { video1 })
                .Returns(new[] { video2 });
            
            _libraryManagerMock.Setup(m => m.GetLibraryOptions(It.IsAny<Video>())).Returns(new Dictionary<string, string>());
            _trickplayManagerMock.Setup(m => m.RefreshTrickplayDataAsync(It.IsAny<Video>(), false, It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
                                .Returns(Task.CompletedTask);

            var cts = new CancellationTokenSource();

            // Act
            await _task.ExecuteAsync(progress, cts.Token);

            // Assert
            _trickplayManagerMock.Verify(m => m.RefreshTrickplayDataAsync(video1, false, It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
            _trickplayManagerMock.Verify(m => m.RefreshTrickplayDataAsync(video2, false, It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.Contains(progressValues, p => Math.Abs(p - 50) < 0.1);
            Assert.Contains(progressValues, p => Math.Abs(p - 100) < 0.1);
        }

        private InternalItemsQuery CreateVideoQuery()
        {
            return new InternalItemsQuery
            {
                MediaTypes = new[] { MediaType.Video },
                SourceTypes = new[] { SourceType.Library },
                IsVirtualItem = false,
                IsFolder = false,
                Recursive = true,
                IncludeOwnedItems = true,
                Limit = 100
            };
        }
    }
}
