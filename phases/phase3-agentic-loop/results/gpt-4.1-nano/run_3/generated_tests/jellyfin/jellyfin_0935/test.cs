using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Providers.Trickplay;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Globalization;

namespace MediaBrowser.Tests.Providers.Trickplay
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

            _localizationMock.Setup(l => l.GetLocalizedString(It.IsAny<string>()))
                .Returns<string>(s => s);

            _task = new TrickplayImagesTask(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _localizationMock.Object,
                _trickplayManagerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var videos = new List<Video> { new Video { Name = "Video1" } };
            var query = new InternalItemsQuery
            {
                MediaTypes = new[] { MediaType.Video },
                SourceTypes = new[] { SourceType.Library },
                IsVirtualItem = false,
                IsFolder = false,
                Recursive = true,
                IncludeOwnedItems = true,
                Limit = 100,
                StartIndex = 0
            };

            _libraryManagerMock.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>()))
                .Returns(1);

            _libraryManagerMock.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(videos);

            _libraryManagerMock.Setup(m => m.GetLibraryOptions(It.IsAny<Video>()))
                .Returns(new object());

            _trickplayManagerMock.Setup(m => m.RefreshTrickplayDataAsync(It.IsAny<Video>(), false, It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            var progress = new Mock<IProgress<double>>();

            // Act
            await _task.ExecuteAsync(progress.Object, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Error creating trickplay files for {ItemName}", "Video1"),
                Times.Once);
        }
    }
}
