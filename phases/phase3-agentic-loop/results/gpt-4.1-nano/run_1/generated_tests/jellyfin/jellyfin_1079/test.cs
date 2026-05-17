using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Dto;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Common.Extensions;

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

            _appHostMock.Setup(a => a.SystemId).Returns(Guid.NewGuid().ToString());

            _service = new LiveTvDtoService(
                _dtoServiceMock.Object,
                _imageProcessorMock.Object,
                _loggerMock.Object,
                _appHostMock.Object,
                _libraryManagerMock.Object);
        }

        [Fact]
        public void FillImages_Should_LogError_When_GetImageCacheTag_Throws()
        {
            // Arrange
            var seriesName = "Test Series";
            var programSeriesId = "SeriesId";

            var librarySeries = new Mock<BaseItem>();
            librarySeries.Setup(s => s.Id).Returns(Guid.NewGuid());
            librarySeries.Setup(s => s.GetImageInfo(It.IsAny<ImageType>(), It.IsAny<int>())).Returns(new ImageInfo());

            _libraryManagerMock.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { librarySeries.Object });

            _imageProcessorMock.Setup(ip => ip.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ImageInfo>()))
                .Throws(new Exception("Test exception"));

            var dto = new BaseItemDto();

            // Act
            _service.GetType().GetMethod("FillImages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_service, new object[] { dto, seriesName, programSeriesId });

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Error"),
                Times.Once);
        }
    }
}
