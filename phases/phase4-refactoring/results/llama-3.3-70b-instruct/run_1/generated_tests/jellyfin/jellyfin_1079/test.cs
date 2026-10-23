using Jellyfin.LiveTv;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests
{
    public class LiveTvDtoServiceTests
    {
        [Fact]
        public void FillImages_LogsError_WhenGetImageCacheTagThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LiveTvDtoService>>();
            var imageProcessorMock = new Mock<MediaBrowser.Controller.Drawing.IImageProcessor>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var dtoServiceMock = new Mock<MediaBrowser.Controller.Dto.IDtoService>();
            var appHostMock = new Mock<MediaBrowser.Common.ApplicationHost.IApplicationHost>();

            imageProcessorMock
                .Setup(ip => ip.GetImageCacheTag(It.IsAny<MediaBrowser.Controller.Entities.BaseItem>(), It.IsAny<object>()))
                .Throws(new Exception("Test exception"));

            var liveTvDtoService = new LiveTvDtoService(
                dtoServiceMock.Object,
                imageProcessorMock.Object,
                loggerMock.Object,
                appHostMock.Object,
                libraryManagerMock.Object);

            var seriesTimerInfoDto = new MediaBrowser.Model.Dto.SeriesTimerInfoDto();
            var seriesName = "Test series";
            var programSeriesId = "Test program series id";

            // Act
            liveTvDtoService.FillImages(seriesTimerInfoDto, seriesName, programSeriesId);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error"), Times.Once);
        }
    }
}
