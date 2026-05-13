using Xunit;
using Moq;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.LiveTv;

namespace Jellyfin.LiveTv.Tests
{
    public class LiveTvDtoServiceTests
    {
        private readonly Mock<ILogger<LiveTvDtoService>> _mockLogger;
        private readonly Mock<IImageProcessor> _mockImageProcessor;
        private readonly Mock<IDtoService> _mockDtoService;
        private readonly Mock<IApplicationHost> _mockAppHost;
        private readonly Mock<ILibraryManager> _mockLibraryManager;
        private readonly LiveTvDtoService _liveTvDtoService;

        public LiveTvDtoServiceTests()
        {
            _mockLogger = new Mock<ILogger<LiveTvDtoService>>();
            _mockImageProcessor = new Mock<IImageProcessor>();
            _mockDtoService = new Mock<IDtoService>();
            _mockAppHost = new Mock<IApplicationHost>();
            _mockLibraryManager = new Mock<ILibraryManager>();
            _liveTvDtoService = new LiveTvDtoService(
                _mockDtoService.Object,
                _mockImageProcessor.Object,
                _mockLogger.Object,
                _mockAppHost.Object,
                _mockLibraryManager.Object);
        }

        [Fact]
        public void FillImages_ShouldLogError_WhenExceptionThrown()
        {
            // Arrange
            var dto = new BaseItemDto();
            var seriesName = "Test Series";
            var programSeriesId = "Test Program Series Id";
            var librarySeries = new Series
            {
                Id = Guid.NewGuid(),
                Name = seriesName
            };
            var imageInfo = new ItemImageInfo
            {
                Type = ImageType.Thumb
            };

            _mockLibraryManager.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { librarySeries }.AsQueryable());

            librarySeries.SetImageInfos(new List<ItemImageInfo> { imageInfo });

            _mockImageProcessor.Setup(m => m.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ItemImageInfo>()))
                .Throws(new Exception("Test Exception"));

            // Act
            _liveTvDtoService.GetType().GetMethod("FillImages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_liveTvDtoService, new object[] { dto, seriesName, programSeriesId });

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
