using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Library;
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
            var video = new Video { Name = "Test Video" };
            var libraryOptions = new LibraryOptions();
            var cancellationToken = new CancellationToken();

            trickplayManagerMock
                .Setup(t => t.RefreshTrickplayDataAsync(video, false, libraryOptions, cancellationToken))
                .Throws(new InvalidOperationException("Test exception"));

            var task = new TrickplayImagesTask(loggerMock.Object, libraryManagerMock.Object, Mock.Of<ILocalizationManager>(), trickplayManagerMock.Object);

            // Act
            await task.ExecuteAsync(new Progress<double>(), cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<InvalidOperationException>(), "Error creating trickplay files for {ItemName}", video.Name), Times.Once);
        }
    }
}
