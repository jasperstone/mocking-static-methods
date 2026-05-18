using Xunit;
using Moq;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
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
        private readonly Mock<ILogger<LiveTvDtoService>> _loggerMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly Mock<IDtoService> _dtoServiceMock;
        private readonly Mock<IApplicationHost> _appHostMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly LiveTvDtoService _service;

        public LiveTvDtoServiceTests()
        {
            _loggerMock = new Mock<ILogger<LiveTvDtoService>>();
            _imageProcessorMock = new Mock<IImageProcessor>();
            _dtoServiceMock = new Mock<IDtoService>();
            _appHostMock = new Mock<IApplicationHost>();
            _libraryManagerMock = new Mock<ILibraryManager>();

            _service = new LiveTvDtoService(
                _dtoServiceMock.Object,
                _imageProcessorMock.Object,
                _loggerMock.Object,
                _appHostMock.Object,
                _libraryManagerMock.Object);
        }

        [Fact]
        public void FillImages_ShouldLogError_WhenExceptionThrown()
        {
            // Arrange
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
                Type = ImageType.Thumb
            };

            librarySeries.SetImageInfos(new List<ItemImageInfo> { imageInfo });

            _libraryManagerMock.Setup(x => x.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { librarySeries });

            _imageProcessorMock.Setup(x => x.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ItemImageInfo>()))
                .Throws(new Exception("Test exception"));

            // Act
            _service.GetType().GetMethod("FillImages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_service, new object[] { dto, seriesName, programSeriesId });

            // Assert
            _loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }
    }
}
