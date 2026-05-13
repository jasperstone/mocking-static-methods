using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Entities;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Sorting;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Trickplay.Tests
{
    public class TrickplayImagesTaskTests
    {
        private static TrickplayImagesTask CreateTask(
            ILogger<TrickplayImagesTask> logger,
            ILibraryManager libraryManager,
            ILocalizationManager localization,
            ITrickplayManager trickplayManager)
        {
            return new TrickplayImagesTask(logger, libraryManager, localization, trickplayManager);
        }

        [Fact]
        public async Task ExecuteAsync_LogsErrorWhenTrickplayRefreshFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayImagesTask>>();
            var progressMock = new Mock<IProgress<double>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var localizationMock = new Mock<ILocalizationManager>();
            var trickplayManagerMock = new Mock<ITrickplayManager>();

            var video = new Video
            {
                Name = "Test Video",
                Id = Guid.NewGuid()
            };

            var libraryOptions = new LibraryOptions();

            var cancellationToken = CancellationToken.None;
            var exception = new InvalidOperationException("Boom");

            localizationMock.Setup(x => x.GetLocalizedString(It.IsAny<string>()))
                .Returns<string>(s => s);

            libraryManagerMock.Setup(x => x.GetCount(It.IsAny<InternalItemsQuery>()))
                .Returns(1);

            libraryManagerMock.Setup(x => x.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new BaseItem[] { video });

            libraryManagerMock.Setup(x => x.GetLibraryOptions(video))
                .Returns(libraryOptions);

            trickplayManagerMock.Setup(x => x.RefreshTrickplayDataAsync(
                    video,
                    false,
                    libraryOptions,
                    cancellationToken))
                .ThrowsAsync(exception);

            var task = CreateTask(
                loggerMock.Object,
                libraryManagerMock.Object,
                localizationMock.Object,
                trickplayManagerMock.Object);

            // Act
            await task.ExecuteAsync(progressMock.Object, cancellationToken);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                        state.ToString()!.Contains("Error creating trickplay files for") &&
                        state.ToString()!.Contains(video.Name)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
