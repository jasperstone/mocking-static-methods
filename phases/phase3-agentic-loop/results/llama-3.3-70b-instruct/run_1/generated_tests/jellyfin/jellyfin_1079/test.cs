using Xunit;
using Moq;
using System;
using System.Globalization;
using Jellyfin.Data.Enums;
using Jellyfin.Extensions;
using MediaBrowser.Common;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.LiveTv;
using Microsoft.Extensions.Logging;

namespace Jellyfin.LiveTv
{
    public class LiveTvDtoServiceTests
    {
        [Fact]
        public void FillImages_LogsError_WhenGetImageCacheTagThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LiveTvDtoService>>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var dtoServiceMock = new Mock<IDtoService>();
            var appHostMock = new Mock<IApplicationHost>();

            imageProcessorMock
                .Setup(ip => ip.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ItemImageInfo>()))
                .Throws(new Exception("Test exception"));

            var librarySeries = new Mock<BaseItem>();
            librarySeries
                .Setup(ls => ls.GetImageInfo(It.IsAny<ImageType>(), It.IsAny<int>()))
                .Returns(new ItemImageInfo { Path = "Test path" });

            libraryManagerMock
                .Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new[] { librarySeries.Object });

            var service = new LiveTvDtoService(
                dtoServiceMock.Object,
                imageProcessorMock.Object,
                loggerMock.Object,
                appHostMock.Object,
                libraryManagerMock.Object);

            var dto = new BaseItemDto();
            var seriesName = "Test series";
            var programSeriesId = "Test program series id";

            // Act
            var timerInfo = new TimerInfo();
            var program = new LiveTvProgram();
            var channel = new BaseItem();
            var liveTvService = new Mock<ILiveTvService>().Object;
            var timerInfoDto = service.GetTimerInfoDto(timerInfo, liveTvService, program, channel);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error"), Times.Once);
        }
    }
}
