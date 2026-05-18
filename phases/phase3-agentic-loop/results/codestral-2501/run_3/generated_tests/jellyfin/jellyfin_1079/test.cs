using Xunit;
using Moq;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.LiveTv;

namespace Jellyfin.LiveTv.Tests
{
    public class LiveTvDtoServiceTests
    {
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly Mock<ILogger<LiveTvDtoService>> _loggerMock;
        private readonly LiveTvDtoService _service;

        public LiveTvDtoServiceTests()
        {
            _libraryManagerMock = new Mock<ILibraryManager>();
            _imageProcessorMock = new Mock<IImageProcessor>();
            _loggerMock = new Mock<ILogger<LiveTvDtoService>>();
            _service = new LiveTvDtoService(
                dtoService: null,
                imageProcessor: _imageProcessorMock.Object,
                logger: _loggerMock.Object,
                appHost: null,
                libraryManager: _libraryManagerMock.Object);
        }

        [Fact]
        public void FillImages_ShouldLogError_WhenGetImageCacheTagThrowsException()
        {
            // Arrange
            var seriesName = "TestSeries";
            var programSeriesId = "TestProgramSeriesId";
            var librarySeries = new Series { Id = Guid.NewGuid() };
            var imageInfo = new ItemImageInfo { Type = ImageType.Thumb, Path = "test" };

            _libraryManagerMock.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { librarySeries });

            _imageProcessorMock.Setup(ip => ip.GetImageCacheTag(librarySeries, imageInfo))
                .Throws(new Exception("Test exception"));

            var dto = new BaseItemDto();

            // Act
            _service.FillImages(dto, seriesName, programSeriesId);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(s => s == "Error")),
                Times.Once);
        }
    }
}
