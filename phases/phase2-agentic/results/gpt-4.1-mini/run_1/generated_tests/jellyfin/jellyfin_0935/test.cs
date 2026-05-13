using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public async Task ExecuteAsync_LogsErrorWhenRefreshTrickplayDataAsyncThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayImagesTask>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var localizationMock = new Mock<ILocalizationManager>();
            var trickplayManagerMock = new Mock<ITrickplayManager>();

            var video = new Video { Name = "TestVideo" };
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
            libraryManagerMock.Setup(lm => lm.GetLibraryOptions(video)).Returns(new LibraryOptions());

            // Setup RefreshTrickplayDataAsync to throw an exception
            var exception = new InvalidOperationException("Test exception");
            trickplayManagerMock.Setup(tm => tm.RefreshTrickplayDataAsync(video, false, It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()))
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
            // Verify LogError was called with the exception and the video name
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error creating trickplay files for TestVideo")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify progress was reported at least once and ended at 100
            Assert.Contains(100d, progressReports);
        }
    }
}
