using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediaBrowser.Providers.Trickplay.Tests
{
    public class TrickplayImagesTaskTests
    {
        private readonly Mock<ILogger<TrickplayImagesTask>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<MediaBrowser.Model.Globalization.ILocalizationManager> _localizationManagerMock;
        private readonly Mock<ITrickplayManager> _trickplayManagerMock;

        public TrickplayImagesTaskTests()
        {
            _loggerMock = new Mock<ILogger<TrickplayImagesTask>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _localizationManagerMock = new Mock<MediaBrowser.Model.Globalization.ILocalizationManager>();
            _trickplayManagerMock = new Mock<ITrickplayManager>();
        }

        [Fact]
        public async Task ExecuteAsync_LogsError_WhenRefreshTrickplayDataAsyncThrowsException()
        {
            // Arrange
            var trickplayImagesTask = new TrickplayImagesTask(_loggerMock.Object, _libraryManagerMock.Object, _localizationManagerMock.Object, _trickplayManagerMock.Object);
            var video = new Video();
            var libraryOptions = new MediaBrowser.Model.Library.LibraryOptions();
            var cancellationToken = new CancellationToken();
            _trickplayManagerMock.Setup(x => x.RefreshTrickplayDataAsync(video, false, libraryOptions, cancellationToken)).Throws<Exception>();

            // Act
            await trickplayImagesTask.ExecuteAsync(new Progress<double>(), cancellationToken);

            // Assert
            _loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), "Error creating trickplay files for {ItemName}", video.Name), Times.Once);
        }
    }
}
