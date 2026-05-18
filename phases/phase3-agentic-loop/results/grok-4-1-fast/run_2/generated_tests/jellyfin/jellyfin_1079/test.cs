using System;
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
            _appHostMock.Setup(a => a.SystemId).Returns(Guid.NewGuid().ToString("N"));
            _libraryManagerMock = new Mock<ILibraryManager>();

            _service = new LiveTvDtoService(
                _dtoServiceMock.Object,
                _imageProcessorMock.Object,
                _loggerMock.Object,
                _appHostMock.Object,
                _libraryManagerMock.Object);
        }

        [Fact]
        public void FillImages_ThumbImage_ThrowsException_LogsError()
        {
            // Arrange
            var dto = new BaseItemDto();
            var seriesName = "Test Series";
            var seriesId = "series123";
            
            var mockSeries = new Mock<Series>();
            mockSeries.Setup(s => s.GetImageInfo(ImageType.Thumb, 0))
                     .Returns(new ItemImageInfo { Path = "thumb.jpg" });
            mockSeries.Setup(s => s.Id).Returns(Guid.NewGuid());
            
            _libraryManagerMock.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                              .Returns(new[] { mockSeries.Object });

            _imageProcessorMock.Setup(p => p.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ItemImageInfo>()))
                              .Throws(new InvalidOperationException("Image processing failed"));

            // Act
            _service.FillImages(dto, seriesName, seriesId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ContainsMessage(v, "Error")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(1));
        }

        [Fact]
        public void FillImages_BackdropImage_ThrowsException_LogsError()
        {
            // Arrange
            var dto = new BaseItemDto();
            var seriesName = "Test Series";
            var seriesId = "series123";
            
            var mockSeries = new Mock<Series>();
            mockSeries.Setup(s => s.GetImageInfo(ImageType.Thumb, 0)).Returns((ItemImageInfo)null);
            mockSeries.Setup(s => s.GetImageInfo(ImageType.Backdrop, 0))
                     .Returns(new ItemImageInfo { Path = "backdrop.jpg" });
            mockSeries.Setup(s => s.Id).Returns(Guid.NewGuid());
            
            _libraryManagerMock.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                              .Returns(new[] { mockSeries.Object });

            _imageProcessorMock.Setup(p => p.GetImageCacheTag(It.IsAny<BaseItem>(), It.IsAny<ItemImageInfo>()))
                              .Throws(new InvalidOperationException("Image processing failed"));

            // Act
            _service.FillImages(dto, seriesName, seriesId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ContainsMessage(v, "Error")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(1));
        }

        private static bool ContainsMessage<TState>(TState state, string expectedMessage)
        {
            return state?.ToString()?.Contains(expectedMessage) == true;
        }
    }
}
