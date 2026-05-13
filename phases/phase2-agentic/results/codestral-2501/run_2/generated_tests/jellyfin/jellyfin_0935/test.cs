using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Trickplay.Tests
{
    public class TrickplayImagesTaskTests
    {
        private readonly Mock<ILogger<TrickplayImagesTask>> _mockLogger;
        private readonly Mock<ILibraryManager> _mockLibraryManager;
        private readonly Mock<ILocalizationManager> _mockLocalizationManager;
        private readonly Mock<ITrickplayManager> _mockTrickplayManager;
        private readonly TrickplayImagesTask _trickplayImagesTask;

        public TrickplayImagesTaskTests()
        {
            _mockLogger = new Mock<ILogger<TrickplayImagesTask>>();
            _mockLibraryManager = new Mock<ILibraryManager>();
            _mockLocalizationManager = new Mock<ILocalizationManager>();
            _mockTrickplayManager = new Mock<ITrickplayManager>();
            _trickplayImagesTask = new TrickplayImagesTask(
                _mockLogger.Object,
                _mockLibraryManager.Object,
                _mockLocalizationManager.Object,
                _mockTrickplayManager.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var video = new Video { Name = "Test Video" };
            var exception = new Exception("Test exception");

            _mockLibraryManager.Setup(lm => lm.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            _mockLibraryManager.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new List<BaseItem> { video });
            _mockLibraryManager.Setup(lm => lm.GetLibraryOptions(It.IsAny<BaseItem>())).Returns(new LibraryOptions());
            _mockTrickplayManager.Setup(tm => tm.RefreshTrickplayDataAsync(It.IsAny<Video>(), It.IsAny<bool>(), It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);

            // Act
            await _trickplayImagesTask.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error creating trickplay files for Test Video")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
