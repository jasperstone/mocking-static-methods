using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Controller.Dto;
using Jellyfin.Controller.Entities;
using Jellyfin.Controller.Library;
using Jellyfin.LiveTv;
using Jellyfin.Model.Dto;
using Jellyfin.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.LiveTv
{
    public class LiveTvDtoServiceTests
    {
        [Fact]
        public void FillImages_LogsError_WhenImageProcessingFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Jellyfin.LiveTv.LiveTvDtoService>>();
            var imageProcessorMock = new Mock<Jellyfin.Controller.Drawing.IImageProcessor>();
            var libraryManagerMock = new Mock<Jellyfin.Controller.Library.ILibraryManager>();

            var dto = new Jellyfin.Model.Dto.SeriesTimerInfoDto();
            var seriesName = "Test Series";
            var programSeriesId = "123";

            var librarySeries = new Jellyfin.Model.Entities.BaseItem
            {
                Id = Guid.NewGuid(),
                GetImageInfo = (imageType, index) => new Jellyfin.Model.Entities.ImageInfo { ImageTag = "testTag" }
            };

            libraryManagerMock.Setup(m => m.GetItemList(It.IsAny<Jellyfin.Controller.Library.InternalItemsQuery>()))
                .Returns(new List<Jellyfin.Model.Entities.BaseItem> { librarySeries });

            imageProcessorMock.Setup(m => m.GetImageCacheTag(It.IsAny<Jellyfin.Model.Entities.BaseItem>(), It.IsAny<Jellyfin.Model.Entities.ImageInfo>()))
                .Throws(new Exception("Image processing error"));

            var service = new Jellyfin.LiveTv.LiveTvDtoService(
                null, // DTO service not needed for this test
                imageProcessorMock.Object,
                loggerMock.Object,
                null, // Application host not needed for this test
                libraryManagerMock.Object);

            // Act
            service.FillImages(dto, seriesName, programSeriesId);

            // Assert
            loggerMock.Verify(m => m.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }
}
