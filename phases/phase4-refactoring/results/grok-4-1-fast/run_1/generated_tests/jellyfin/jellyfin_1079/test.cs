using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.LiveTv;
using Microsoft.Extensions.Logging;
using Moq;
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
            
            var mockAppHost = new Mock<IApplicationHost>();
            mockAppHost.Setup(h => h.SystemId).Returns(Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
            
            _mockLibraryManager = new Mock<ILibraryManager>();

            _service = new LiveTvDtoService(
                _mockDtoService.Object,
                _mockImageProcessor.Object,
                _mockLogger.Object,
                mockAppHost.Object,
                _mockLibraryManager.Object);
        }

        [Fact]
        public void FillImages_ThumbImageProcessorThrowsException_LogsError()
        {
            // Arrange
            var seriesName = "Test Series";
            var seriesId = "series123";
            var mockSeries = new Mock<Series>();
            mockSeries.Setup(s => s.Id).Returns(Guid.NewGuid());
            var mockImageInfo = new Mock<ImageInfo>();

            _mockLibraryManager.Setup(m => m.GetItemList(It.Is<InternalItemsQuery>(q =>
                q.IncludeItemTypes.Contains(BaseItemKind.Series) &&
                q.Name == seriesName &&
                q.Limit == 1 &&
                q.ImageTypes.Contains(ImageType.Thumb))))
                .Returns(new[] { mockSeries.Object });

            mockSeries.Setup(s => s.GetImageInfo(ImageType.Thumb, 0)).Returns(mockImageInfo.Object);
            _mockImageProcessor.Setup(p => p.GetImageCacheTag(mockSeries.Object, mockImageInfo.Object))
                .Throws(new InvalidOperationException("Image processing failed"));

            // Act
            _service.GetSeriesTimerInfoDto(new SeriesTimerInfo { Name = seriesName, SeriesId = seriesId }, null!, "Channel");

            // Assert
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void FillImages_BackdropImageProcessorThrowsException_LogsError()
        {
            // Arrange
            var seriesName = "Test Series";
            var seriesId = "series123";
            var mockSeries = new Mock<Series>();
            mockSeries.Setup(s => s.Id).Returns(Guid.NewGuid());
            var mockImageInfo = new Mock<ImageInfo>();

            // Thumb query - return series but no thumb image
            _mockLibraryManager.Setup(m => m.GetItemList(It.Is<InternalItemsQuery>(q =>
                q.IncludeItemTypes.Contains(BaseItemKind.Series) &&
                q.Name == seriesName &&
                q.ImageTypes.Contains(ImageType.Thumb))))
                .Returns(new[] { mockSeries.Object });

            mockSeries.Setup(s => s.GetImageInfo(ImageType.Thumb, 0)).Returns((ImageInfo)null);

            // Backdrop query
            _mockLibraryManager.Setup(m => m.GetItemList(It.Is<InternalItemsQuery>(q =>
                q.IncludeItemTypes.Contains(BaseItemKind.Series) &&
                q.Name == seriesName &&
                q.ImageTypes.Contains(ImageType.Backdrop))))
                .Returns(new[] { mockSeries.Object });

            mockSeries.Setup(s => s.GetImageInfo(ImageType.Backdrop, 0)).Returns(mockImageInfo.Object);
            _mockImageProcessor.Setup(p => p.GetImageCacheTag(mockSeries.Object, mockImageInfo.Object))
                .Throws(new InvalidOperationException("Backdrop processing failed"));

            // Act
            _service.GetSeriesTimerInfoDto(new SeriesTimerInfo { Name = seriesName, SeriesId = seriesId }, null!, "Channel");

            // Assert
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
