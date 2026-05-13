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
            _mockLibraryManager.Setup(manager => manager.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            _mockLibraryManager.Setup(manager => manager.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new List<BaseItem> { video });
            _mockTrickplayManager.Setup(manager => manager.RefreshTrickplayDataAsync(It.IsAny<Video>(), It.IsAny<bool>(), It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            // Act
            await _trickplayImagesTask.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            _mockLogger.Verify(logger => logger.LogError(It.IsAny<EventId>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
