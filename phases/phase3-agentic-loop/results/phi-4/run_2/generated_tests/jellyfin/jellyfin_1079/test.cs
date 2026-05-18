using Moq;
using Xunit;
using Jellyfin.LiveTv;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.LiveTv;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Jellyfin.Tests.LiveTv
{
    public class LiveTvDtoServiceTests
    {
        private class MockBaseItem : BaseItem
        {
            public override ImageInfo GetImageInfo(ImageType imageType, int index)
            {
                return new ImageInfo { ImageTag = "testTag" };
            }
        }

        [Fact]
        public void FillImages_LogsError_WhenImageProcessingFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LiveTvDtoService>>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var dto = new SeriesTimerInfoDto();
            var seriesName = "Test Series";
            var programSeriesId = "123";

            // Simulate library series with images
            var librarySeries = new MockBaseItem
            {
                Id = Guid.NewGuid()
            };

            libraryManagerMock.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { librarySeries });

            var service = new LiveTvDtoService(
                null, // DTO service not needed for this test
                imageProcessorMock.Object,
                loggerMock.Object,
                null, // Application host not needed for this test
                libraryManagerMock.Object);

            // Simulate exception during image processing
            imageProcessorMock.Setup(m => m.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ImageInfo>()))
                .Throws(new InvalidOperationException("Image processing error"));

            // Act
            service.FillImages(dto, seriesName, programSeriesId);

            // Assert
            loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), "Error"),
                Times.Exactly(2)); // Two calls to LogError in FillImages
        }
    }
}
