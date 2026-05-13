using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Dto;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Controller.Dto;

namespace Jellyfin.LiveTv.Tests
{
    public class LiveTvDtoServiceTests
    {
        private readonly Mock<ILogger<LiveTvDtoService>> _loggerMock;
        private readonly Mock<IImageProcessor> _imageProcessorMock;
        private readonly Mock<IDtoService> _dtoServiceMock;
        private readonly Mock<IApplicationHost> _appHostMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;

        public LiveTvDtoServiceTests()
        {
            _loggerMock = new Mock<ILogger<LiveTvDtoService>>();
            _imageProcessorMock = new Mock<IImageProcessor>();
            _dtoServiceMock = new Mock<IDtoService>();
            _appHostMock = new Mock<IApplicationHost>();
            _libraryManagerMock = new Mock<ILibraryManager>();
        }

        [Fact]
        public void FillImages_Should_LogError_When_GetImageCacheTag_ThrowsException_ForThumb()
        {
            // Arrange
            var service = new LiveTvDtoService(
                _dtoServiceMock.Object,
                _imageProcessorMock.Object,
                _loggerMock.Object,
                _appHostMock.Object,
                _libraryManagerMock.Object);

            var dto = new BaseItemDto();
            var seriesName = "Test Series";
            var programSeriesId = "SeriesId";

            var librarySeriesMock = new Mock<BaseItem>();
            librarySeriesMock.SetupGet(s => s.Id).Returns(Guid.NewGuid());
            librarySeriesMock.Setup(s => s.GetImageInfo(ImageType.Thumb, 0))
                .Returns(new ImageInfo());

            _libraryManagerMock.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { librarySeriesMock.Object });

            _imageProcessorMock.Setup(ip => ip.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ImageInfo>()))
                .Throws(new Exception("Test Exception"));

            // Act
            service.GetType().GetMethod("FillImages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(service, new object[] { dto, seriesName, programSeriesId });

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.Is<string>(s => s == "Error")),
                Times.Once);
        }

        [Fact]
        public void FillImages_Should_LogError_When_GetImageCacheTag_ThrowsException_ForBackdrop()
        {
            // Arrange
            var service = new LiveTvDtoService(
                _dtoServiceMock.Object,
                _imageProcessorMock.Object,
                _loggerMock.Object,
                _appHostMock.Object,
                _libraryManagerMock.Object);

            var dto = new BaseItemDto();
            var seriesName = "Test Series";
            var programSeriesId = "SeriesId";

            var librarySeriesMock = new Mock<BaseItem>();
            librarySeriesMock.SetupGet(s => s.Id).Returns(Guid.NewGuid());
            var imageInfo = new ImageInfo();

            librarySeriesMock.Setup(s => s.GetImageInfo(ImageType.Thumb, 0))
                .Returns(imageInfo);
            librarySeriesMock.Setup(s => s.GetImageInfo(ImageType.Backdrop, 0))
                .Returns(imageInfo);

            _libraryManagerMock.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { librarySeriesMock.Object });

            _imageProcessorMock.Setup(ip => ip.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ImageInfo>()))
                .Throws(new Exception("Test Exception"));

            // Act
            service.GetType().GetMethod("FillImages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(service, new object[] { dto, seriesName, programSeriesId });

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.Is<string>(s => s == "Error")),
                Times.Once);
        }
    }
}
