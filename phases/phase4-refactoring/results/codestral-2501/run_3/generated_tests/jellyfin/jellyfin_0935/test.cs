using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Providers.Trickplay.Tests
{
    public class TrickplayImagesTaskTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TrickplayImagesTask>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var localizationMock = new Mock<ILocalizationManager>();
            var trickplayManagerMock = new Mock<ITrickplayManager>();

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

            var video = new Video { Name = "Test Video" };
            var videos = new List<BaseItem> { video };

            libraryManagerMock.Setup(lm => lm.GetCount(query)).Returns(1);
            libraryManagerMock.Setup(lm => lm.GetItemList(query)).Returns(videos);
            libraryManagerMock.Setup(lm => lm.GetLibraryOptions(video)).Returns(new LibraryOptions());

            trickplayManagerMock.Setup(tm => tm.RefreshTrickplayDataAsync(video, false, It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("Test Exception"));

            var task = new TrickplayImagesTask(loggerMock.Object, libraryManagerMock.Object, localizationMock.Object, trickplayManagerMock.Object);
            var progress = new Mock<IProgress<double>>();

            // Act
            await task.ExecuteAsync(progress.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ),
                Times.Once);
        }
    }
}
