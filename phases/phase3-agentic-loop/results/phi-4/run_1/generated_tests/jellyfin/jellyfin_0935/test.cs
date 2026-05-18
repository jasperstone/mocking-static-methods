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
using MediaBrowser.Model; // Added using directive for LibraryOptions

namespace MediaBrowser.Providers.Trickplay.Tests
{
    public class TrickplayImagesTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TrickplayImagesTask>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLocalization = new Mock<ILocalizationManager>();
            var mockTrickplayManager = new Mock<ITrickplayManager>();

            var video = new Video { Name = "Test Video" };
            var libraryOptions = new LibraryOptions();

            mockLibraryManager
                .Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>()))
                .Returns(1);

            mockLibraryManager
                .Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<Video> { video });

            mockLibraryManager
                .Setup(m => m.GetLibraryOptions(It.IsAny<Video>()))
                .Returns(libraryOptions);

            mockTrickplayManager
                .Setup(m => m.RefreshTrickplayDataAsync(It.IsAny<Video>(), false, It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            var task = new TrickplayImagesTask(mockLogger.Object, mockLibraryManager.Object, mockLocalization.Object, mockTrickplayManager.Object);

            var progress = new Mock<IProgress<double>>();
            var cancellationToken = new CancellationToken();

            // Act
            await task.ExecuteAsync(progress.Object, cancellationToken);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<InvalidOperationException>(),
                    "Error creating trickplay files for {ItemName}",
                    video.Name),
                Times.Once);
        }
    }
}
