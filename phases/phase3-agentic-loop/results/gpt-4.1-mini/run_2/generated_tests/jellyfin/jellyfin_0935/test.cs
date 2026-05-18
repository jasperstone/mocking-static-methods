using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Providers.Trickplay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Trickplay.Tests
{
    public class TrickplayImagesTaskTests
    {
        private class DummyLibraryOptions { }

        [Fact]
        public async Task ExecuteAsync_LogsError_WhenRefreshTrickplayDataAsyncThrows()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TrickplayImagesTask>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLocalization = new Mock<ILocalizationManager>();
            var mockTrickplayManager = new Mock<ITrickplayManager>();

            var video1 = new Video { Name = "Video1" };
            var video2 = new Video { Name = "Video2" };

            // Setup localization strings
            mockLocalization.Setup(l => l.GetLocalizedString(It.IsAny<string>())).Returns((string key) => key);

            // Setup library manager to return 2 videos
            mockLibraryManager.Setup(l => l.GetCount(It.IsAny<InternalItemsQuery>())).Returns(2);
            mockLibraryManager.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { video1, video2 });
            // Return dummy library options object
            mockLibraryManager.Setup(l => l.GetLibraryOptions(It.IsAny<BaseItem>())).Returns(new DummyLibraryOptions());

            // Setup trickplay manager to throw on first video, succeed on second
            mockTrickplayManager.Setup(t => t.RefreshTrickplayDataAsync(video1, false, It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));
            mockTrickplayManager.Setup(t => t.RefreshTrickplayDataAsync(video2, false, It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var task = new TrickplayImagesTask(
                mockLogger.Object,
                mockLibraryManager.Object,
                mockLocalization.Object,
                mockTrickplayManager.Object);

            var progressReports = new List<double>();
            var progress = new Progress<double>(p => progressReports.Add(p));
            var cancellationToken = CancellationToken.None;

            // Act
            await task.ExecuteAsync(progress, cancellationToken);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error creating trickplay files for Video1")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Also verify progress reported 100 at the end
            Assert.Contains(100d, progressReports);
        }
    }
}
