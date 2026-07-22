using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Localization;
using MediaBrowser.Controller.Trickplay;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediaBrowser.Providers.Trickplay.Tests
{
    public class TrickplayImagesTaskTests
    {
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<ILocalizationManager> _localizationManagerMock;
        private readonly Mock<ITrickplayManager> _trickplayManagerMock;
        private readonly Mock<ILogger<TrickplayImagesTask>> _loggerMock;

        public TrickplayImagesTaskTests()
        {
            _libraryManagerMock = new Mock<ILibraryManager>();
            _localizationManagerMock = new Mock<ILocalizationManager>();
            _trickplayManagerMock = new Mock<ITrickplayManager>();
            _loggerMock = new Mock<ILogger<TrickplayImagesTask>>();
        }

        [Fact]
        public async Task ExecuteAsync_LogsError_WhenRefreshTrickplayDataAsyncThrowsException()
        {
            // Arrange
            var trickplayImagesTask = new TrickplayImagesTask(_loggerMock.Object, _libraryManagerMock.Object, _localizationManagerMock.Object, _trickplayManagerMock.Object);
            var video = new Video { Name = "Test Video" };
            var libraryOptions = new LibraryOptions();
            var cancellationToken = new CancellationToken();
            _trickplayManagerMock.Setup(x => x.RefreshTrickplayDataAsync(video, false, libraryOptions, cancellationToken)).Throws(new Exception("Test exception"));

            // Act
            await trickplayImagesTask.ExecuteAsync(new Progress<double>(), cancellationToken);

            // Assert
            _loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), "Error creating trickplay files for {ItemName}", video.Name), Times.Once);
        }
    }
}
