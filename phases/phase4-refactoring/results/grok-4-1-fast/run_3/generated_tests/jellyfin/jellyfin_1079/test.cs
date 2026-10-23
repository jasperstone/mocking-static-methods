using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Drawing;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.LiveTv;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
            _appHostMock.Setup(x => x.SystemId).Returns(Guid.NewGuid());
            _libraryManagerMock = new Mock<ILibraryManager>();
        }

        [Fact]
        public void FillImages_ThumbImageProcessorThrowsException_LogsError()
        {
            // Arrange
            var service = CreateService();
            var dto = new BaseItemDto();
            var seriesName = "Test Series";
            var seriesId = "series123";
            var series = new Mock<Series>();
            series.Setup(s => s.Name).Returns(seriesName);
            series.Setup(s => s.Id).Returns(Guid.NewGuid());
            var imageInfo = new ItemImageInfo();

            _libraryManagerMock
                .Setup(m => m.GetItemList(It.Is<InternalItemsQuery>(q => q.Name == seriesName)))
                .Returns(new BaseItem[] { series.Object });

            series.Setup(s => s.GetImageInfo(ImageType.Thumb, 0)).Returns(imageInfo);

            _imageProcessorMock
                .Setup(m => m.GetImageCacheTag(series.Object, imageInfo))
                .Throws(new InvalidOperationException("Test exception"));

            // Act
            service.FillImages(dto, seriesName, seriesId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyFormat<string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void FillImages_BackdropImageProcessorThrowsException_LogsError()
        {
            // Arrange
            var service = CreateService();
            var dto = new BaseItemDto();
            var seriesName = "Test Series";
            var seriesId = "series123";
            var series = new Mock<Series>();
            series.Setup(s => s.Name).Returns(seriesName);
            series.Setup(s => s.Id).Returns(Guid.NewGuid());
            var imageInfo = new ItemImageInfo();

            _libraryManagerMock
                .Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new BaseItem[] { series.Object });

            // Thumb no image
            series.Setup(s => s.GetImageInfo(ImageType.Thumb, 0)).Returns((ItemImageInfo)null);

            // Backdrop throws
            series.Setup(s => s.GetImageInfo(ImageType.Backdrop, 0)).Returns(imageInfo);
            _imageProcessorMock
                .Setup(m => m.GetImageCacheTag(series.Object, imageInfo))
                .Throws(new InvalidOperationException("Test exception"));

            // Act
            service.FillImages(dto, seriesName, seriesId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyFormat<string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
                Times.Once);
        }

        private LiveTvDtoService CreateService()
        {
            return new LiveTvDtoService(
                _dtoServiceMock.Object,
                _imageProcessorMock.Object,
                _loggerMock.Object,
                _appHostMock.Object,
                _libraryManagerMock.Object);
        }
    }
}
