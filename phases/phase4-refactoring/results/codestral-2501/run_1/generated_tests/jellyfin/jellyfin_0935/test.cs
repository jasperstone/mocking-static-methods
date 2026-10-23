using Xunit;
using Moq;
using MediaBrowser.Providers.Trickplay;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using MediaBrowser.Model.Configuration;

namespace MediaBrowser.Tests.Providers.Trickplay
{
    public class TrickplayImagesTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldLogError_WhenRefreshTrickplayDataAsyncThrowsException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TrickplayImagesTask>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockTrickplayManager = new Mock<ITrickplayManager>();
            var mockVideo = new Mock<Video>();
            var mockProgress = new Mock<IProgress<double>>();
            var cancellationToken = new CancellationToken();

            mockVideo.Setup(v => v.Name).Returns("TestVideo");
            mockLibraryManager.Setup(lm => lm.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            mockLibraryManager.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new List<BaseItem> { mockVideo.Object });
            mockLibraryManager.Setup(lm => lm.GetLibraryOptions(It.IsAny<BaseItem>())).Returns(new LibraryOptions());
            mockTrickplayManager.Setup(tm => tm.RefreshTrickplayDataAsync(It.IsAny<Video>(), It.IsAny<bool>(), It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("Test exception"));

            var task = new TrickplayImagesTask(mockLogger.Object, mockLibraryManager.Object, null, mockTrickplayManager.Object);

            // Act
            await task.ExecuteAsync(mockProgress.Object, cancellationToken);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Error creating trickplay files for {ItemName}",
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }
    }
}
