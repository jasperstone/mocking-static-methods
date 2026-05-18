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

namespace Jellyfin.LiveTv.Tests
{
    public class LiveTvDtoServiceTests
    {
        private readonly Mock<ILogger<LiveTvDtoService>> _loggerMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly Mock<IDtoService> _dtoServiceMock;
        private readonly Mock<IApplicationHost> _appHostMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;

        public LiveTvDtoServiceTests()
        {
            _loggerMock = new Mock<ILogger<LiveTvDtoService>>();
            _imageProcessorMock = new Mock<IImageProcessor>();
            _dtoServiceMock = new Mock<IDtoService>();
            _appHostMock = new Mock<IApplicationHost>();
            _libraryManagerMock = new Mock<ILibraryManager>();
        }

        [Fact]
        public void FillImages_LogsError_WhenGetImageCacheTagThrowsException()
        {
            // Arrange
            var librarySeries = new Mock<MediaBrowser.Controller.Entities.BaseItem>();
            var image = new Mock<MediaBrowser.Controller.Entities.ItemImageInfo>();
            var dto = new BaseItemDto();
            var seriesName = "Series Name";
            var programSeriesId = "Program Series Id";

            _libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new[] { librarySeries.Object });
            librarySeries.Setup(s => s.GetImageInfo(It.IsAny<ImageType>(), It.IsAny<int>())).Returns(image.Object);
            _imageProcessorMock.Setup(p => p.GetImageCacheTag(It.IsAny<MediaBrowser.Controller.Entities.BaseItem>(), It.IsAny<MediaBrowser.Controller.Entities.ItemImageInfo>())).Throws(new Exception("Test exception"));

            var service = new LiveTvDtoService(_dtoServiceMock.Object, _imageProcessorMock.Object, _loggerMock.Object, _appHostMock.Object, _libraryManagerMock.Object);

            // Act
            var baseItemDto = new BaseItemDto();
            var seriesTimerInfo = new SeriesTimerInfo();
            seriesTimerInfo.Name = seriesName;
            seriesTimerInfo.SeriesId = programSeriesId;
            var seriesTimerInfoDto = new SeriesTimerInfoDto();
            service.GetSeriesTimerInfoDto(seriesTimerInfo, null, null);

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error"), Times.Once);
        }

        [Fact]
        public void FillImages_LogsError_WhenGetImageCacheTagForBackdropThrowsException()
        {
            // Arrange
            var librarySeries = new Mock<MediaBrowser.Controller.Entities.BaseItem>();
            var image = new Mock<MediaBrowser.Controller.Entities.ItemImageInfo>();
            var dto = new BaseItemDto();
            var seriesName = "Series Name";
            var programSeriesId = "Program Series Id";

            _libraryManagerMock.Setup(l => l.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new[] { librarySeries.Object });
            librarySeries.Setup(s => s.GetImageInfo(It.IsAny<ImageType>(), It.IsAny<int>())).Returns(image.Object);
            _imageProcessorMock.Setup(p => p.GetImageCacheTag(It.IsAny<MediaBrowser.Controller.Entities.BaseItem>(), It.IsAny<MediaBrowser.Controller.Entities.ItemImageInfo>())).Throws(new Exception("Test exception"));

            var service = new LiveTvDtoService(_dtoServiceMock.Object, _imageProcessorMock.Object, _loggerMock.Object, _appHostMock.Object, _libraryManagerMock.Object);

            // Act
            var baseItemDto = new BaseItemDto();
            var seriesTimerInfo = new SeriesTimerInfo();
            seriesTimerInfo.Name = seriesName;
            seriesTimerInfo.SeriesId = programSeriesId;
            var seriesTimerInfoDto = new SeriesTimerInfoDto();
            service.GetSeriesTimerInfoDto(seriesTimerInfo, null, null);

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error"), Times.Exactly(2));
        }
    }
}
