using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Jellyfin.LiveTv;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.LiveTv;
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
            var thumbImageMock = new Mock<ImageInfo>();
            var backdropImageMock = new Mock<ImageInfo>();

            // Setup library manager to return a list with one librarySeries
            libraryManagerMock.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { librarySeriesMock.Object });

            // Setup librarySeries to return images for Thumb and Backdrop
            librarySeriesMock.Setup(ls => ls.GetImageInfo(ImageType.Thumb, 0)).Returns(thumbImageMock.Object);
            librarySeriesMock.Setup(ls => ls.GetImageInfo(ImageType.Backdrop, 0)).Returns(backdropImageMock.Object);

            // Setup librarySeries Id
            var seriesId = Guid.NewGuid();
            librarySeriesMock.SetupGet(ls => ls.Id).Returns(seriesId);

            // Setup imageProcessor to throw on GetImageCacheTag for Thumb and Backdrop
            imageProcessorMock.Setup(ip => ip.GetImageCacheTag(librarySeriesMock.Object, thumbImageMock.Object))
                .Throws(new InvalidOperationException("Thumb image error"));
            imageProcessorMock.Setup(ip => ip.GetImageCacheTag(librarySeriesMock.Object, backdropImageMock.Object))
                .Throws(new InvalidOperationException("Backdrop image error"));

            // Act
            // We call the private FillImages method via reflection because it's private
            var fillImagesMethod = typeof(LiveTvDtoService).GetMethod("FillImages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fillImagesMethod.Invoke(service, new object[] { dto, "seriesName", "programSeriesId" });

            // Assert
            // Verify that LogError was called twice with exceptions
            loggerMock.Verify(
                x => x.LogError(It.Is<Exception>(ex => ex.Message == "Thumb image error"), "Error"),
                Times.Once);

            loggerMock.Verify(
                x => x.LogError(It.Is<Exception>(ex => ex.Message == "Backdrop image error"), "Error"),
                Times.Once);
        }
    }
}
