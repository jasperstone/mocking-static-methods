using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Trickplay.Tests
{
    public class TrickplayImagesTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsErrorWhenRefreshTrickplayDataAsyncThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayImagesTask>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var localizationMock = new Mock<ILocalizationManager>();
            var trickplayManagerMock = new Mock<ITrickplayManager>();

            var video = new Video { Name = "TestVideo" };
            var videos = new List<BaseItem> { video };

            libraryManagerMock.Setup(m => m.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            libraryManagerMock.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(videos);
            libraryManagerMock.Setup(m => m.GetLibraryOptions(video)).Returns((MediaBrowser.Model.Configuration.LibraryOptions)null);

            var exception = new InvalidOperationException("Test exception");
            trickplayManagerMock
                .Setup(m => m.RefreshTrickplayDataAsync(video, false, It.IsAny<MediaBrowser.Model.Configuration.LibraryOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(exception);

            var task = new TrickplayImagesTask(
                loggerMock.Object,
                libraryManagerMock.Object,
                localizationMock.Object,
                trickplayManagerMock.Object);

            var progressReports = new List<double>();
            var progress = new Progress<double>(p => progressReports.Add(p));
            var cancellationToken = CancellationToken.None;

            // Act
            await task.ExecuteAsync(progress, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    exception,
                    "Error creating trickplay files for {ItemName}",
                    video.Name),
                Times.Once);

            Assert.Contains(100, progressReports);
        }
    }
}
