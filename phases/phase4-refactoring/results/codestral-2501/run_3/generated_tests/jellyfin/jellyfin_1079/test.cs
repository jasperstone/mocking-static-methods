using Xunit;
using Moq;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Dto;
using MediaBrowser.Controller.Drawing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.LiveTv;

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

        var librarySeries = new Series
        {
            Id = Guid.NewGuid(),
            Name = seriesName
        };

        var imageInfo = new ItemImageInfo
        {
            Type = ImageType.Thumb,
            Path = "testPath"
        };

        librarySeries.SetImageInfos(new List<ItemImageInfo> { imageInfo });

        mockLibraryManager.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>()))
            .Returns(new List<BaseItem> { librarySeries });

        mockImageProcessor.Setup(ip => ip.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ItemImageInfo>()))
            .Throws(new InvalidOperationException("Test exception"));

        var service = new LiveTvDtoService(
            null,
            mockImageProcessor.Object,
            mockLogger.Object,
            null,
            mockLibraryManager.Object
        );

        // Act
        service.FillImages(dto, seriesName, programSeriesId);

        // Assert
        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("Error")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }
}
