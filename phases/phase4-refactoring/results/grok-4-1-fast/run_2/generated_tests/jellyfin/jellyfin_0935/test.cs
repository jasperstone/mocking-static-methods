using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Tasks;
using MediaBrowser.Providers.Trickplay;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Trickplay.Tests
{
    public class TrickplayImagesTaskTests
    {
        private readonly Mock<ILogger<TrickplayImagesTask>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<ILocalizationManager> _localizationMock;
        private readonly Mock<ITrickplayManager> _trickplayManagerMock;
        private readonly TrickplayImagesTask _task;

        public TrickplayImagesTaskTests()
        {
            _loggerMock = new Mock<ILogger<TrickplayImagesTask>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _localizationMock = new Mock<ILocalizationManager>();
            _trickplayManagerMock = new Mock<ITrickplayManager>();
            
            _task = new TrickplayImagesTask(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _localizationMock.Object,
                _trickplayManagerMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenRefreshTrickplayDataAsyncThrowsException_LogsErrorWithItemName()
        {
            // Arrange
            var video = new Video { Name = "Test Video", Id = Guid.NewGuid() };
            var query = new InternalItemsQuery
            {
                MediaTypes = new[] { MediaType.Video },
                SourceTypes = new[] { SourceType.Library },
                IsVirtualItem = false,
                IsFolder = false,
                Recursive = true,
                IncludeOwnedItems = true,
                Limit = 100
            };
            var libraryOptions = new LibraryOptions();

            _libraryManagerMock.Setup(m => m.GetCount(query)).Returns(1);
            _libraryManagerMock.Setup(m => m.GetItemList(query)).Returns(new[] { video }.Cast<BaseItem>().ToList().AsReadOnly());
            _libraryManagerMock.Setup(m => m.GetLibraryOptions(video)).Returns(libraryOptions);
            _trickplayManagerMock.Setup(m => m.RefreshTrickplayDataAsync(video, false, libraryOptions, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            _localizationMock.Setup(m => m.GetLocalizedString(It.IsAny<string>())).Returns("Test");

            var progress = new Progress<double>();
            var cancellationToken = new CancellationToken();

            // Act
            await _task.ExecuteAsync(progress, cancellationToken);

            // Assert
            _trickplayManagerMock.Verify(m => m.RefreshTrickplayDataAsync(video, false, libraryOptions, It.IsAny<CancellationToken>()), Times.Once);
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error creating trickplay files for Test Video")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WhenNoVideosExists_CompletesWithoutError()
        {
            // Arrange
            var query = new InternalItemsQuery
            {
                MediaTypes = new[] { MediaType.Video },
                SourceTypes = new[] { SourceType.Library },
                IsVirtualItem = false,
                IsFolder = false,
                Recursive = true,
                IncludeOwnedItems = true,
                Limit = 100
            };

            _libraryManagerMock.Setup(m => m.GetCount(query)).Returns(0);
            _localizationMock.Setup(m => m.GetLocalizedString(It.IsAny<string>())).Returns("Test");

            var progress = new Progress<double>();
            var cancellationToken = new CancellationToken();

            // Act
            await _task.ExecuteAsync(progress, cancellationToken);

            // Assert
            _trickplayManagerMock.Verify(m => m.RefreshTrickplayDataAsync(It.IsAny<Video>(), It.IsAny<bool>(), It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
            _loggerMock.VerifyNoOtherCalls();
        }
    }
}
