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
            var imageMock = new Mock<ImageInfo>();

            // Setup library manager to return a list with one BaseItem
            libraryManagerMock.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { librarySeriesMock.Object });

            // Setup librarySeries to return an image for Thumb and Backdrop
            librarySeriesMock.Setup(ls => ls.GetImageInfo(ImageType.Thumb, 0)).Returns(imageMock.Object);
            librarySeriesMock.Setup(ls => ls.GetImageInfo(ImageType.Backdrop, 0)).Returns(imageMock.Object);

            // Setup librarySeries Id
            var seriesId = Guid.NewGuid();
            librarySeriesMock.SetupGet(ls => ls.Id).Returns(seriesId);

            // Setup imageProcessor to throw on GetImageCacheTag to trigger catch block
            imageProcessorMock.Setup(ip => ip.GetImageCacheTag(librarySeriesMock.Object, imageMock.Object))
                .Throws(new InvalidOperationException("Test exception"));

            // Act
            // We call the public method GetSeriesTimerInfoDto which calls FillImages internally
            var info = new SeriesTimerInfo
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test Series",
                Days = new[] { DayOfWeek.Monday },
                SeriesId = Guid.NewGuid().ToString()
            };

            var liveTvServiceMock = new Mock<ILiveTvService>();
            liveTvServiceMock.SetupGet(s => s.Name).Returns("TestService");

            // Call method under test
            var result = service.GetSeriesTimerInfoDto(info, liveTvServiceMock.Object, "Test Channel");

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

            // Also verify that the dto ParentThumbItemId and ParentBackdropItemId are set to seriesId despite exceptions
            Assert.Equal(seriesId, result.ParentThumbItemId);
            Assert.Equal(seriesId, result.ParentBackdropItemId);
        }
    }
}
