using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests
{
    public class LiveTvDtoServiceTests
    {
        private readonly Mock<ILogger<LiveTvDtoService>> _loggerMock;
        private readonly Mock<IDtoService> _dtoServiceMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly LiveTvDtoService _service;

        public LiveTvDtoServiceTests()
        {
            _loggerMock = new Mock<ILogger<LiveTvDtoService>>();
            _dtoServiceMock = new Mock<IDtoService>();
            _libraryManagerMock = new Mock<ILibraryManager>();

            // Create minimal mocks for required dependencies
            var imageProcessorMock = new Mock<IImageProcessor>();
            var appHostMock = new Mock<IApplicationHost>();

            _service = new LiveTvDtoService(
                _dtoServiceMock.Object,
                imageProcessorMock.Object,
                _loggerMock.Object,
                appHostMock.Object,
                _libraryManagerMock.Object);
        }

        [Fact]
        public void FillImages_ThumbImage_ThrowsException_LogsError()
        {
            // Arrange
            var dto = new BaseItemDto();
            var seriesName = "Test Series";
            var seriesId = "series123";
            
            var series = new Mock<Series>();
            series.Setup(s => s.Id).Returns(Guid.NewGuid());
            series.Setup(s => s.Name).Returns(seriesName);
            var imageInfo = new ItemImageInfo();

            var thumbQuery = new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.Series },
                Name = seriesName,
                Limit = 1,
                ImageTypes = new[] { ImageType.Thumb },
                DtoOptions = new DtoOptions(false)
            };

            _libraryManagerMock
                .Setup(m => m.GetItemList(It.Is<InternalItemsQuery>(q => 
                    q.ImageTypes.Contains(ImageType.Thumb))))
                .Returns(new[] { series.Object });

            series.Setup(s => s.GetImageInfo(ImageType.Thumb, 0)).Returns(imageInfo);

            // Mock ImageProcessor to throw exception
            var imageProcessorMock = new Mock<IImageProcessor>();
            imageProcessorMock
                .Setup(m => m.GetImageCacheTag(It.IsAny<Series>(), It.IsAny<ItemImageInfo>()))
                .Throws(new InvalidOperationException("Test exception"));

            // Recreate service with throwing image processor
            _service = new LiveTvDtoService(
                _dtoServiceMock.Object,
                imageProcessorMock.Object,
                _loggerMock.Object,
                new Mock<IApplicationHost>().Object,
                _libraryManagerMock.Object);

            // Act
            _service.FillImages(dto, seriesName, seriesId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
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
            
            var series = new Mock<Series>();
            series.Setup(s => s.Id).Returns(Guid.NewGuid());
            series.Setup(s => s.Name).Returns(seriesName);
            var thumbImageInfo = new ItemImageInfo();
            var backdropImageInfo = new ItemImageInfo();

            var thumbQuery = new InternalItemsQuery
            {
                IncludeItemTypes = new[] { BaseItemKind.Series },
                Name = seriesName,
                Limit = 1,
                ImageTypes = new[] { ImageType.Thumb },
                DtoOptions = new DtoOptions(false)
            };

            _libraryManagerMock
                .Setup(m => m.GetItemList(It.Is<InternalItemsQuery>(q => 
                    q.ImageTypes.Contains(ImageType.Thumb))))
                .Returns(new[] { series.Object });

            series.Setup(s => s.GetImageInfo(ImageType.Thumb, 0)).Returns(thumbImageInfo);
            series.Setup(s => s.GetImageInfo(ImageType.Backdrop, 0)).Returns(backdropImageInfo);

            var imageProcessorMock = new Mock<IImageProcessor>();
            imageProcessorMock
                .SetupSequence(m => m.GetImageCacheTag(It.IsAny<Series>(), It.IsAny<ItemImageInfo>()))
                .Returns("thumb-tag")
                .Throws(new InvalidOperationException("Test exception"));

            _service = new LiveTvDtoService(
                _dtoServiceMock.Object,
                imageProcessorMock.Object,
                _loggerMock.Object,
                new Mock<IApplicationHost>().Object,
                _libraryManagerMock.Object);

            // Act
            _service.FillImages(dto, seriesName, seriesId);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(1));
        }

        [Fact]
        public void FillImages_NoSeriesFound_NoErrorLogged()
        {
            // Arrange
            var dto = new BaseItemDto();
            var seriesName = "NonExistent";

            _libraryManagerMock
                .Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(Enumerable.Empty<BaseItem>());

            // Act
            _service.FillImages(dto, seriesName, null);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
