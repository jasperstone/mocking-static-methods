using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Drawing;

namespace Jellyfin.LiveTv.Tests
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
                .Setup(ip => ip.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ImageInfo>()))
                .Throws(new Exception("Test exception"));

            var liveTvDtoService = new LiveTvDtoService(
                dtoServiceMock.Object,
                imageProcessorMock.Object,
                loggerMock.Object,
                appHostMock.Object,
                libraryManagerMock.Object);

            var seriesTimerInfoDto = new SeriesTimerInfoDto();
            var seriesName = "Test series";
            var programSeriesId = "Test program series id";

            // Act
            liveTvDtoService.FillImages(seriesTimerInfoDto, seriesName, programSeriesId);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error"), Times.Once);
        }
    }
}
