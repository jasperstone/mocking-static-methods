using Xunit;
using Moq;
using MediaBrowser.Providers.Trickplay;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System;
using MediaBrowser.Controller.Entities;
using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Providers.Trickplay.Tests
{
    public class TrickplayImagesTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TrickplayImagesTask>>();
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLocalization = new Mock<ILocalizationManager>();
            var mockTrickplayManager = new Mock<ITrickplayManager>();

            var video = new Video { Name = "Test Video" };
            var videos = new List<Video> { video }.AsQueryable();

            mockLibraryManager.Setup(lm => lm.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            mockLibraryManager.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(videos);
            mockLibraryManager.Setup(lm => lm.GetLibraryOptions(It.IsAny<Video>())).Returns(new Model.Configuration.LibraryOptions());

            mockTrickplayManager.Setup(tm => tm.RefreshTrickplayDataAsync(It.IsAny<Video>(), It.IsAny<bool>(), It.IsAny<Model.Configuration.LibraryOptions>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("Test Exception"));

            var task = new TrickplayImagesTask(mockLogger.Object, mockLibraryManager.Object, mockLocalization.Object, mockTrickplayManager.Object);
            var progress = new Mock<IProgress<double>>();

            // Act
            await task.ExecuteAsync(progress.Object, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }
    }
}
