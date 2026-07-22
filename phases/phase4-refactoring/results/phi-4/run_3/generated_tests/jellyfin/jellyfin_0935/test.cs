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
        [Fact]
        public async Task ExecuteAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TrickplayImagesTask>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLocalization = new Mock<ILocalizationManager>();
            var mockTrickplayManager = new Mock<ITrickplayManager>();

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

            mockLibraryManager.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            mockLibraryManager.Setup(m => m.GetItemList(query)).Returns(new List<Video> { video });
            mockLibraryManager.Setup(m => m.GetLibraryOptions(video)).Returns(new LibraryOptions());

            mockTrickplayManager.Setup(m => m.RefreshTrickplayDataAsync(video, false, It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            var task = new TrickplayImagesTask(mockLogger.Object, mockLibraryManager.Object, mockLocalization.Object, mockTrickplayManager.Object);

            // Act
            await task.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            mockLogger.Verify(
                m => m.LogError(It.IsAny<Exception>(), "Error creating trickplay files for {ItemName}", video.Name),
                Times.Once);
        }
    }
}
