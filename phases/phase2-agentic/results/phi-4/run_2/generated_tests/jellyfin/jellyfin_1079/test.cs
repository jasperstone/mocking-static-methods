using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Jellyfin.Controller.Entities;
using Jellyfin.Controller.Library;
using Jellyfin.Controller.LiveTv;
using Jellyfin.Model.Dto;
using Jellyfin.Model.Entities;
using System;
using System.Globalization;

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
            var mockAppHost = new Mock<IApplicationHost>();

            var dto = new BaseItemDto();
            var seriesName = "Test Series";
            var programSeriesId = "123";

            // Simulate an exception when calling GetImageCacheTag
            mockImageProcessor
                .Setup(x => x.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ImageInfo>()))
                .Throws(new Exception("Test Exception"));

            var service = new LiveTvDtoService(
                null, // IDtoService is not used in this test
                mockImageProcessor.Object,
                mockLogger.Object,
                mockAppHost.Object,
                mockLibraryManager.Object);

            // Act
            service.FillImages(dto, seriesName, programSeriesId);

            // Assert
            mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(s => s == "Error")),
                Times.Once);
        }
    }
}
