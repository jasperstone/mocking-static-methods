using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controllers.Trickplay;
using MediaBrowser.Model.Globalization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Trickplay.Tests
{
    public class TrickplayImagesTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenTrickplayRefreshThrows_LogsErrorWithVideoName()
        {
            var mockLogger = new Mock<ILogger<TrickplayImagesTask>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLocalization = new Mock<ILocalizationManager>();
            var mockTrickplayManager = new Mock<ITrickplayManager>();

            var video = new TestVideo { Name = "Sample Video" };
            var testException = new InvalidOperationException("boom");

            mockLibraryManager
                .Setup(l => l.GetCount(It.IsAny<InternalItemsQuery>()))
                .Returns(1);

            mockLibraryManager
                .Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new BaseItem[] { video });

            mockLibraryManager
                .Setup(l => l.GetLibraryOptions(video))
                .Returns((LibraryOptions)null);

            mockTrickplayManager
                .Setup(t => t.RefreshTrickplayDataAsync(video, false, null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(testException);

            var progressValues = new List<double>();
            var progress = new Progress<double>(v => progressValues.Add(v));

            var task = new TrickplayImagesTask(
                mockLogger.Object,
                mockLibraryManager.Object,
                mockLocalization.Object,
                mockTrickplayManager.Object);

            await task.ExecuteAsync(progress, CancellationToken.None);

            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, type) =>
                        state.ToString()!.Contains("Error creating trickplay files for") &&
                        state.ToString()!.Contains(video.Name!)),
                    testException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Equal(new[] { 100d }, progressValues.Where(v => Math.Abs(v - 100d) < 0.001));
        }

        private class TestVideo : Video
        {
        }
    }
}
