using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Localization;
using MediaBrowser.Controller.Trickplay;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Providers.Trickplay.Tests
{
    public class TrickplayImagesTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsError_WhenRefreshTrickplayDataAsyncFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayImagesTask>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var trickplayManagerMock = new Mock<ITrickplayManager>();
            var localizationManagerMock = new Mock<ILocalizationManager>();

            var video = new Video { Name = "Test Video" };

            libraryManagerMock
                .Setup(l => l.GetCount(It.IsAny<InternalItemsQuery>()))
                .Returns(1);

            libraryManagerMock
                .Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new[] { video });

            trickplayManagerMock
                .Setup(t => t.RefreshTrickplayDataAsync(It.IsAny<Video>(), It.IsAny<bool>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("Test exception"));

            var task = new TrickplayImagesTask(loggerMock.Object, libraryManagerMock.Object, localizationManagerMock.Object, trickplayManagerMock.Object);

            // Act
            await task.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error creating trickplay files for {ItemName}", video.Name), Times.Once);
        }
    }
}
