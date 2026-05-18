using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Globalization;
using Microsoft.Extensions.Logging;
using MediaBrowser.Providers.Trickplay;
using MediaBrowser.Controller.Entities;
using System.Collections.Generic;
using System.Linq;
using System;
using Jellyfin.Data.Enums;
using MediaBrowser.Model.Configuration;

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

            var video = new Video { Name = "Test Video" };
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

            libraryManagerMock.Setup(lm => lm.GetCount(query)).Returns(1);
            libraryManagerMock.Setup(lm => lm.GetItemList(query)).Returns(new List<BaseItem> { video });
            trickplayManagerMock.Setup(tm => tm.RefreshTrickplayDataAsync(video, false, It.IsAny<LibraryOptions>(), It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("Test exception"));

            var task = new TrickplayImagesTask(
                loggerMock.Object,
                libraryManagerMock.Object,
                localizationMock.Object,
                trickplayManagerMock.Object
            );

            var progressMock = new Mock<IProgress<double>>();

            // Act
            await task.ExecuteAsync(progressMock.Object, CancellationToken.None);

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Error creating trickplay files for {ItemName}",
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }
    }
}
