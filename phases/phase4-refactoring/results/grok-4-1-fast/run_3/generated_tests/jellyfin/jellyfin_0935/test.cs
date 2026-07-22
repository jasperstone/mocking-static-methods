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
using MediaBrowser.Providers.Trickplay;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace MediaBrowser.Providers.Trickplay.Tests
{
    public class TrickplayImagesTaskTests
    {
        private readonly Mock<ILogger<TrickplayImagesTask>> _mockLogger;
        private readonly Mock<ILibraryManager> _mockLibraryManager;
        private readonly Mock<ILocalizationManager> _mockLocalizationManager;
        private readonly Mock<ITrickplayManager> _mockTrickplayManager;
        private readonly TrickplayImagesTask _task;

        public TrickplayImagesTaskTests()
        {
            _mockLogger = new Mock<ILogger<TrickplayImagesTask>>();
            _mockLibraryManager = new Mock<ILibraryManager>();
            _mockLocalizationManager = new Mock<ILocalizationManager>();
            _mockTrickplayManager = new Mock<ITrickplayManager>();

            _task = new TrickplayImagesTask(
                _mockLogger.Object,
                _mockLibraryManager.Object,
                _mockLocalizationManager.Object,
                _mockTrickplayManager.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenRefreshTrickplayDataAsyncThrowsException_LogsErrorWithItemName()
        {
            // Arrange
            var video = new Video { Name = "Test Video", Id = Guid.NewGuid() };
            var progress = new Progress<double>();
            var cancellationToken = new CancellationToken();

            _mockLibraryManager.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            _mockLibraryManager.SetupSequence(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new[] { video });
            _mockLibraryManager.Setup(m => m.GetLibraryOptions(video))
                .Returns(new Dictionary<string, string>());

            _mockTrickplayManager.Setup(m => m.RefreshTrickplayDataAsync(
                    video, false, It.IsAny<IDictionary<string, string>>(), cancellationToken))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Act
            await _task.ExecuteAsync(progress, cancellationToken);

            // Assert
            _mockTrickplayManager.Verify(m => m.RefreshTrickplayDataAsync(
                video, false, It.IsAny<IDictionary<string, string>>(), cancellationToken), Times.Once);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Test Video")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenNoVideosExists_DoesNotLogError()
        {
            // Arrange
            var progress = new Progress<double>();
            var cancellationToken = new CancellationToken();

            _mockLibraryManager.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(0);

            // Act
            await _task.ExecuteAsync(progress, cancellationToken);

            // Assert
            _mockTrickplayManager.Verify(m => m.RefreshTrickplayDataAsync(
                It.IsAny<Video>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, string>>(), 
                It.IsAny<CancellationToken>()), Times.Never);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
