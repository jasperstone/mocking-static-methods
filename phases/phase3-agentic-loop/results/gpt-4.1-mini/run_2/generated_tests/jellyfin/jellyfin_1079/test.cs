using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Jellyfin.Data.Enums;
using Jellyfin.LiveTv;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests
{
    public class LiveTvDtoServiceTests
    {
        [Fact]
        public void FillImages_LogsError_WhenGetImageCacheTagThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LiveTvDtoService>>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var dtoServiceMock = new Mock<IDtoService>();
            var appHostMock = new Mock<IApplicationHost>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var service = new LiveTvDtoService(
                dtoServiceMock.Object,
                imageProcessorMock.Object,
                loggerMock.Object,
                appHostMock.Object,
                libraryManagerMock.Object);

            var dto = new SeriesTimerInfoDto();

            var librarySeriesMock = new Mock<BaseItem>();
            var imageInfoMock = new Mock<MediaBrowser.Controller.Entities.ItemImageInfo>();

            // Setup library manager to return a list with one librarySeries
            libraryManagerMock.Setup(x => x.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { librarySeriesMock.Object });

            // Setup librarySeries to return an image for Thumb and Backdrop
            librarySeriesMock.Setup(x => x.GetImageInfo(ImageType.Thumb, 0)).Returns(imageInfoMock.Object);
            librarySeriesMock.Setup(x => x.GetImageInfo(ImageType.Backdrop, 0)).Returns(imageInfoMock.Object);

            // Setup librarySeries Id
            var seriesId = Guid.NewGuid();
            librarySeriesMock.SetupGet(x => x.Id).Returns(seriesId);

            // Setup imageProcessor to throw when GetImageCacheTag is called
            imageProcessorMock.Setup(x => x.GetImageCacheTag(librarySeriesMock.Object, imageInfoMock.Object))
                .Throws(new InvalidOperationException("Test exception"));

            // Act
            // We need to call the private FillImages method via reflection
            var fillImagesMethod = typeof(LiveTvDtoService).GetMethod("FillImages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fillImagesMethod.Invoke(service, new object[] { dto, "seriesName", "programSeriesId" });

            // Assert
            // Verify that LogError was called twice (once for Thumb, once for Backdrop)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }
    }
}
