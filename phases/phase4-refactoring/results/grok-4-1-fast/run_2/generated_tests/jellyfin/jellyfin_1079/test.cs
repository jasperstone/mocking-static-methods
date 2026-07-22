using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Jellyfin.Data.Enums;
using MediaBrowser.Common;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.LiveTv;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Jellyfin.LiveTv.Tests
{
    public class LiveTvDtoServiceTests
    {
        private readonly Mock<ILogger<LiveTvDtoService>> _mockLogger;
        private readonly Mock<IImageProcessor> _mockImageProcessor;
        private readonly Mock<IDtoService> _mockDtoService;
        private readonly Mock<IApplicationHost> _mockAppHost;
        private readonly Mock<ILibraryManager> _mockLibraryManager;
        private readonly LiveTvDtoService _service;

        public LiveTvDtoServiceTests()
        {
            _mockLogger = new Mock<ILogger<LiveTvDtoService>>();
            _mockImageProcessor = new Mock<IImageProcessor>();
            _mockDtoService = new Mock<IDtoService>();
            _mockAppHost = new Mock<IApplicationHost>();
            _mockLibraryManager = new Mock<ILibraryManager>();

            _service = new LiveTvDtoService(
                _mockDtoService.Object,
                _mockImageProcessor.Object,
                _mockLogger.Object,
                _mockAppHost.Object,
                _mockLibraryManager.Object);
        }

        [Fact]
        public void GetSeriesTimerInfoDto_ThumbImageProcessorThrows_LogsError()
        {
            // Arrange
            const string seriesName = "Test Series";
            const string seriesId = "test-series-id";

            var mockSeries = new Mock<BaseItem>();
            var mockSeriesId = Guid.NewGuid();
            mockSeries.Setup(s => s.Id).Returns(mockSeriesId);
            mockSeries.Setup(s => s.GetImageInfo(ImageType.Thumb, 0))
                      .Returns(new ItemImageInfo { Path = "/path/to/thumb.jpg" });

            var thumbQuery = new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.Series },
                Name = seriesName,
                Limit = 1,
                ImageTypes = new[] { ImageType.Thumb },
                DtoOptions = new DtoOptions(false)
            };

            _mockLibraryManager.Setup(m => m.GetItemList(It.Is<InternalItemsQuery>(q => 
                q.IncludeItemTypes.SequenceEqual(thumbQuery.IncludeItemTypes) &&
                q.Name == thumbQuery.Name &&
                q.Limit == thumbQuery.Limit &&
                q.ImageTypes.SequenceEqual(thumbQuery.ImageTypes))))
                               .Returns(new[] { mockSeries.Object });

            _mockImageProcessor.Setup(p => p.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ItemImageInfo>()))
                               .Throws(new InvalidOperationException("Image processing failed"));

            var mockLiveTvService = new Mock<ILiveTvService>();

            // Act
            var timerInfo = new SeriesTimerInfo { Name = seriesName, SeriesId = seriesId };
            _service.GetSeriesTimerInfoDto(timerInfo, mockLiveTvService.Object, "channel");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void GetSeriesTimerInfoDto_BackdropImageProcessorThrows_LogsError()
        {
            // Arrange
            const string seriesName = "Test Series";
            const string seriesId = "test-series-id";

            var mockSeries = new Mock<BaseItem>();
            var mockSeriesId = Guid.NewGuid();
            mockSeries.Setup(s => s.Id).Returns(mockSeriesId);
            mockSeries.Setup(s => s.GetImageInfo(ImageType.Thumb, 0)).Returns((ItemImageInfo)null);
            mockSeries.Setup(s => s.GetImageInfo(ImageType.Backdrop, 0))
                      .Returns(new ItemImageInfo { Path = "/path/to/backdrop.jpg" });

            var thumbQuery = new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.Series },
                Name = seriesName,
                Limit = 1,
                ImageTypes = new[] { ImageType.Thumb },
                DtoOptions = new DtoOptions(false)
            };

            _mockLibraryManager.Setup(m => m.GetItemList(It.Is<InternalItemsQuery>(q => 
                q.IncludeItemTypes.SequenceEqual(thumbQuery.IncludeItemTypes) &&
                q.Name == thumbQuery.Name &&
                q.Limit == thumbQuery.Limit &&
                q.ImageTypes.SequenceEqual(thumbQuery.ImageTypes))))
                               .Returns(new[] { mockSeries.Object });

            _mockImageProcessor.Setup(p => p.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ItemImageInfo>()))
                               .Throws(new InvalidOperationException("Backdrop processing failed"));

            var mockLiveTvService = new Mock<ILiveTvService>();

            // Act
            var timerInfo = new SeriesTimerInfo { Name = seriesName, SeriesId = seriesId };
            _service.GetSeriesTimerInfoDto(timerInfo, mockLiveTvService.Object, "channel");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
