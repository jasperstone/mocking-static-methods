using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Jellyfin.LiveTv;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests
{
    public class LiveTvDtoServiceTests
    {
        [Fact]
        public void FillImages_LogsError_WhenGetImageCacheTagThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LiveTvDtoService>>();
            var imageProcessorMock = new Mock<IImageProcessor>();
            var dtoServiceMock = new Mock<IDtoService>();
            var appHostMock = new Mock<IApplicationHost>();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var service = new LiveTvDtoService(
                dtoServiceMock.Object,
                imageProcessorMock.Object,
                loggerMock.Object,
                appHostMock.Object,
                libraryManagerMock.Object);

            var dto = new SeriesTimerInfoDto();

            var librarySeriesMock = new Mock<BaseItem>();
            var thumbImageMock = new Mock<ImageInfo>();
            var backdropImageMock = new Mock<ImageInfo>();

            // Setup library manager to return a list with one librarySeries
            libraryManagerMock.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { librarySeriesMock.Object });

            // Setup librarySeries to return thumb and backdrop images
            librarySeriesMock.Setup(ls => ls.GetImageInfo(ImageType.Thumb, 0)).Returns(thumbImageMock.Object);
            librarySeriesMock.Setup(ls => ls.GetImageInfo(ImageType.Backdrop, 0)).Returns(backdropImageMock.Object);

            // Setup librarySeries Id
            var seriesId = Guid.NewGuid();
            librarySeriesMock.SetupGet(ls => ls.Id).Returns(seriesId);

            // Setup imageProcessor to throw on GetImageCacheTag for thumb image
            imageProcessorMock.Setup(ip => ip.GetImageCacheTag(librarySeriesMock.Object, thumbImageMock.Object))
                .Throws(new InvalidOperationException("Thumb image error"));

            // Setup imageProcessor to throw on GetImageCacheTag for backdrop image
            imageProcessorMock.Setup(ip => ip.GetImageCacheTag(librarySeriesMock.Object, backdropImageMock.Object))
                .Throws(new InvalidOperationException("Backdrop image error"));

            // Act
            // We call the public method that calls FillImages internally.
            // Use GetSeriesTimerInfoDto which calls FillImages.
            var seriesTimerInfo = new SeriesTimerInfo
            {
                Id = "id",
                Name = "seriesName",
                Days = new[] { DayOfWeek.Monday },
                SeriesId = "seriesId"
            };
            var liveTvServiceMock = new Mock<ILiveTvService>();
            liveTvServiceMock.SetupGet(s => s.Name).Returns("serviceName");

            // We call GetSeriesTimerInfoDto which calls FillImages internally
            var result = service.GetSeriesTimerInfoDto(seriesTimerInfo, liveTvServiceMock.Object, "channelName");

            // Assert
            // Verify that LogError was called twice (once for each exception)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }
    }

    // Minimal stubs for missing types
    public class SeriesTimerInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DayOfWeek[] Days { get; set; }
        public string SeriesId { get; set; }
    }

    public interface IImageProcessor
    {
        string GetImageCacheTag(BaseItem item, ImageInfo image);
    }

    public interface IDtoService
    {
        BaseItemDto GetBaseItemDto(LiveTvProgram program, DtoOptions options);
    }

    public interface IApplicationHost
    {
        Guid SystemId { get; }
    }

    public interface ILibraryManager
    {
        IEnumerable<BaseItem> GetItemList(InternalItemsQuery query);
    }

    public class InternalItemsQuery
    {
        public string[] IncludeItemTypes { get; set; }
        public string Name { get; set; }
        public int Limit { get; set; }
        public ImageType[] ImageTypes { get; set; }
        public DtoOptions DtoOptions { get; set; }
        public string ExternalSeriesId { get; set; }
    }

    public class BaseItem
    {
        public virtual Guid Id { get; set; }
        public virtual ImageInfo GetImageInfo(ImageType type, int index) => null;
    }

    public class ImageInfo { }

    public enum ImageType
    {
        Thumb,
        Backdrop,
        Primary
    }

    public class SeriesTimerInfoDto : BaseItemDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ChannelName { get; set; }
        public string[] Days { get; set; }
        public string DayPattern { get; set; }
        public string[] ParentBackdropImageTags { get; set; }
        public Guid ParentThumbItemId { get; set; }
        public Guid ParentBackdropItemId { get; set; }
    }

    public class BaseItemDto
    {
        public string ParentThumbImageTag { get; set; }
        public Guid ParentThumbItemId { get; set; }
        public string[] ParentBackdropImageTags { get; set; }
        public Guid ParentBackdropItemId { get; set; }
    }

    public class DtoOptions
    {
        public DtoOptions(bool flag) { }
    }

    public class LiveTvProgram { }

    public interface ILiveTvService
    {
        string Name { get; }
    }
}
