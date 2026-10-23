using Xunit;
using Moq;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.LiveTv;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Querying;

public class LiveTvDtoServiceTests
{
    [Fact]
    public void FillImages_ShouldLogError_WhenExceptionIsThrown()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<LiveTvDtoService>>();
        var mockImageProcessor = new Mock<IImageProcessor>();
        var mockLibraryManager = new Mock<ILibraryManager>();

        var dto = new BaseItemDto();
        var seriesName = "TestSeries";
        var programSeriesId = "TestProgramSeriesId";

        var librarySeries = new Mock<BaseItem>();
        var imageInfo = new ItemImageInfo();

        mockLibraryManager.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
            .Returns(new List<BaseItem> { librarySeries.Object });

        mockImageProcessor.Setup(m => m.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ItemImageInfo>()))
            .Throws(new Exception("Test exception"));

        var service = new LiveTvDtoService(
            null,
            mockImageProcessor.Object,
            mockLogger.Object,
            null,
            mockLibraryManager.Object);

        // Act
        service.FillImages(dto, seriesName, programSeriesId);

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()
            ),
            Times.Once);
    }
}
