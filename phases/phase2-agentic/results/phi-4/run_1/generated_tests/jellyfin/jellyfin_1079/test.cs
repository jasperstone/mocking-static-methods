using System;
using System.Globalization;
using System.Linq;
using Moq;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Jellyfin.LiveTv.Tests
{
    public class LiveTvDtoServiceTests
    {
        [Fact]
        public void FillImages_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LiveTvDtoService>>();
            var mockImageProcessor = new Mock<IImageProcessor>();
            var mockLibraryManager = new Mock<ILibraryManager>();

            var dto = new BaseItemDto();
            var seriesName = "Test Series";
            var programSeriesId = "123";

            mockLibraryManager.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new[] { new Mock<BaseItem>().Object });

            mockImageProcessor.Setup(m => m.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ImageInfo>()))
                .Throws(new Exception("Test exception"));

            var service = new LiveTvDtoService(
                null, // Mock IDtoService
                mockImageProcessor.Object,
                mockLogger.Object,
                null, // Mock IApplicationHost
                mockLibraryManager.Object);

            // Act
            service.FillImages(dto, seriesName, programSeriesId);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(s => s == "Error")),
                Times.Once);
        }
    }
}
