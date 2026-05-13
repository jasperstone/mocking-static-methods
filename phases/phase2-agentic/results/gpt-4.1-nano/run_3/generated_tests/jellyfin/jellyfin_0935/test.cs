using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Providers.Trickplay;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Trickplay;

namespace MediaBrowser.Tests.Providers.Trickplay
{
    public class TrickplayImagesTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_Should_LogError_When_ExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayImagesTask>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var localizationMock = new Mock<ILocalizationManager>();
            var trickplayManagerMock = new Mock<ITrickplayManager>();
            var progressMock = new Mock<IProgress<double>>();
            var cancellationToken = new CancellationToken();

            // Setup localization
            localizationMock.Setup(l => l.GetLocalizedString(It.IsAny<string>()))
                .Returns<string>(s => s);

            // Setup library manager
            var videos = new List<Video> { new Video { Name = "TestVideo" } };
            var query = new InternalItemsQuery
            {
                MediaTypes = new[] { MediaType.Video },
                SourceTypes = new[] { SourceType.Library },
                IsVirtualItem = false,
                IsFolder = false,
                Recursive = true,
                IncludeOwnedItems = true,
                Limit = 100,
                StartIndex = 0
            };

            libraryManagerMock.Setup(l => l.GetCount(It.IsAny<InternalItemsQuery>()))
                .Returns(1);
            libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(videos);

            // Setup GetLibraryOptions to throw exception
            libraryManagerMock.Setup(l => l.GetLibraryOptions(It.IsAny<Video>()))
                .Throws(new Exception("Test exception"));

            // Setup trickplay manager
            trickplayManagerMock.Setup(t => t.RefreshTrickplayDataAsync(It.IsAny<Video>(), false, It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var task = new TrickplayImagesTask(
                loggerMock.Object,
                libraryManagerMock.Object,
                localizationMock.Object,
                trickplayManagerMock.Object);

            // Act
            await task.ExecuteAsync(progressMock.Object, cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Error creating trickplay files for {ItemName}", "TestVideo"),
                Times.Once);
        }
    }
}
