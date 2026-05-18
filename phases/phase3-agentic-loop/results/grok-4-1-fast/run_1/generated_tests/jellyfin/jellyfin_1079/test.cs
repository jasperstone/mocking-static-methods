using System;
using System.Collections.Generic;
using System.Linq;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Data.Enums;

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
        public void FillImages_ThumbImage_ThrowsException_LogsError()
        {
            // Arrange
            var dto = new BaseItemDto();
            var seriesName = "Test Series";
            var seriesId = "series123";
            
            var mockSeries = new Mock<Series>();
            mockSeries.Setup(s => s.GetImageInfo(ImageType.Thumb, 0))
                      .Returns(new ItemImageInfo { Path = "thumb.jpg" });

            var query = new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.Series },
                Name = seriesName,
                Limit = 1,
                ImageTypes = new[] { ImageType.Thumb },
                DtoOptions = new DtoOptions(false)
            };
            
            _libraryManagerMock.Setup(m => m.GetItemList(query))
                               .Returns(new[] { mockSeries.Object });

            _imageProcessorMock.Setup(p => p.GetImageCacheTag(mockSeries.Object, It.IsAny<ItemImageInfo>()))
                               .Throws(new InvalidOperationException("Test exception"));

            // Act
            var fillImagesMethod = typeof(LiveTvDtoService).GetMethod("FillImages", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fillImagesMethod!.Invoke(_service, new object[] { dto, seriesName, seriesId });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>>(v => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
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

            var query = new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.Series },
                Name = seriesName,
                Limit = 1,
                ImageTypes = new[] { ImageType.Thumb },
                DtoOptions = new DtoOptions(false)
            };
            
            _libraryManagerMock.Setup(m => m.GetItemList(query))
                               .Returns(new[] { mockSeries.Object });

            _imageProcessorMock.Setup(p => p.GetImageCacheTag(mockSeries.Object, It.IsAny<ItemImageInfo>()))
                               .Throws(new InvalidOperationException("Test exception"));

            // Act
            var fillImagesMethod = typeof(LiveTvDtoService).GetMethod("FillImages", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fillImagesMethod!.Invoke(_service, new object[] { dto, seriesName, seriesId });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>>(v => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Exactly(1));
        }
    }
}
