using Moq;
using Xunit;
using Jellyfin.LiveTv;
using Jellyfin.Controller.Entities;
using Jellyfin.Controller.Library;
using Jellyfin.Model.Entities;
using Jellyfin.Model.Dto;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;

public class LiveTvDtoServiceTests
{
    [Fact]
    public void FillImages_LogsError_WhenImageProcessingFails()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LiveTvDtoService>>();
        var mockImageProcessor = new Mock<IImageProcessor>();
        var mockLibraryManager = new Mock<ILibraryManager>();

        var librarySeries = new BaseItem
        {
            Id = Guid.NewGuid(),
            GetImageInfo = (imageType, index) => new ImageInfo { ImageTag = "testTag" }
        };

        mockLibraryManager
            .Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
            .Returns(new List<BaseItem> { librarySeries });

        mockImageProcessor
            .Setup(m => m.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ImageInfo>()))
            .Throws(new Exception("Image processing error"));

        var service = new LiveTvDtoService(
            null, // Mock IDtoService
            mockImageProcessor.Object,
            mockLogger.Object,
            null, // Mock IApplicationHost
            mockLibraryManager.Object);

        var dto = new SeriesTimerInfoDto();

        // Act
        service.FillImages(dto, "Test Series", "testSeriesId");

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<Exception>(),
                It.Is<string>(s => s == "Error")),
            Times.Once);
    }
}
