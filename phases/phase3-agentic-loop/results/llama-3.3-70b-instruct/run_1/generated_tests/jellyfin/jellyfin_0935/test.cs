using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Providers.Trickplay;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Providers.Tests
{
    public class TrickplayImagesTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayImagesTask>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var localizationManagerMock = new Mock<ILocalizationManager>();
            var trickplayManagerMock = new Mock<ITrickplayManager>();

            var task = new TrickplayImagesTask(loggerMock.Object, libraryManagerMock.Object, localizationManagerMock.Object, trickplayManagerMock.Object);

            var video = new Video { Name = "Test Video" };
            libraryManagerMock.Setup(l => l.GetCount(It.IsAny<InternalItemsQuery>())).Returns(1);
            libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new[] { video });
            trickplayManagerMock.Setup(t => t.RefreshTrickplayDataAsync(video, false, It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>())).Throws(new Exception("Test exception"));

            // Act
            await task.ExecuteAsync(new Progress<double>(), CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error creating trickplay files for {ItemName}", video.Name), Times.Once);
        }
    }
}
