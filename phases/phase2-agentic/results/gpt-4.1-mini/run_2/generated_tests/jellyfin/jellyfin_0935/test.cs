using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
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
            var localizationMock = new Mock<MediaBrowser.Model.Globalization.ILocalizationManager>();
            var trickplayManagerMock = new Mock<ITrickplayManager>();

            var video = new Video { Name = "Test Video" };
            var videos = new List<BaseItem> { video };

            var queryCaptured = new List<InternalItemsQuery>();

            // Setup GetCount to return 1 video
            libraryManagerMock.Setup(lm => lm.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);

            // Setup GetItemList to return our single video
            libraryManagerMock.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns((InternalItemsQuery q) =>
                {
                    queryCaptured.Add(q);
                    return videos;
                });

            // Setup GetLibraryOptions to return some dummy options
            libraryManagerMock.Setup(lm => lm.GetLibraryOptions(It.IsAny<BaseItem>()))
                .Returns(new LibraryOptions());

            // Setup RefreshTrickplayDataAsync to throw an exception to trigger LogError
            trickplayManagerMock.Setup(tm => tm.RefreshTrickplayDataAsync(
                It.IsAny<Video>(), 
                It.IsAny<bool>(), 
                It.IsAny<LibraryOptions>(), 
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Setup localization to return dummy strings for Name and Description
            localizationMock.Setup(l => l.GetLocalizedString(It.IsAny<string>())).Returns((string key) => key);

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
            // Verify LogError was called once with the expected exception and message
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<InvalidOperationException>(ex => ex.Message == "Test exception"),
                    "Error creating trickplay files for {ItemName}",
                    "Test Video"),
                Times.Once);

            // Verify progress was reported at least twice (partial and 100%)
            Assert.Contains(100d, progressReports);
            Assert.True(progressReports.Count >= 2);

            // Verify the query's StartIndex was set to 0 (since only one video)
            Assert.Contains(queryCaptured, q => q.StartIndex == 0);
        }
    }
}
