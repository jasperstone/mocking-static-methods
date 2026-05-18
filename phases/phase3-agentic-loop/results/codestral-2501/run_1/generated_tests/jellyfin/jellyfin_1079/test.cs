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
using MediaBrowser.Common;
using MediaBrowser.Controller.Dto;

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
        public void FillImages_ShouldLogError_WhenGetImageCacheTagThrowsException()
        {
            // Arrange
            var dto = new BaseItemDto();
            var seriesName = "TestSeries";
            var programSeriesId = "TestProgramSeriesId";
            var librarySeries = new Series { Id = Guid.NewGuid() };
            var image = new ItemImageInfo { Type = ImageType.Thumb, Path = "test" };

            _mockLibraryManager.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { librarySeries });

            librarySeries.SetImageInfos(new List<ItemImageInfo> { image });

            _mockImageProcessor.Setup(ip => ip.GetImageCacheTag(librarySeries, image))
                .Throws(new Exception("Test exception"));

            // Act
            _liveTvDtoService.GetType().GetMethod("FillImages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_liveTvDtoService, new object[] { dto, seriesName, programSeriesId });

            // Assert
            _mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
