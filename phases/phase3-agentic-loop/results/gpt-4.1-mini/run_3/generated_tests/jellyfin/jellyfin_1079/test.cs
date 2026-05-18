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
using MediaBrowser.Model.LiveTv;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests
{
    public class LiveTvDtoServiceTests
    {
        private class DummyApplicationHost : MediaBrowser.Common.IApplicationHost
        {
            public Guid SystemId { get; } = Guid.NewGuid();
            public string ApplicationName => "DummyApp";
            public string ApplicationVersion => "1.0";
            public string ApplicationExePath => "";
            public string ApplicationExeName => "";
            public string ApplicationDataPath => "";
            public string ApplicationCachePath => "";
            public string ApplicationTempPath => "";
            public string ApplicationLogPath => "";
            public string ApplicationConfigPath => "";
            public string ApplicationPluginsPath => "";
            public string ApplicationWebPath => "";
            public string ApplicationWebClientPath => "";
            public string ApplicationWebClientCachePath => "";
            public string ApplicationWebClientConfigPath => "";
            public string ApplicationWebClientPluginsPath => "";
            public string ApplicationWebClientTempPath => "";
            public string ApplicationWebClientLogPath => "";
            public string ApplicationWebClientDataPath => "";
            public string ApplicationWebClientCachePath2 => "";
            public string ApplicationWebClientConfigPath2 => "";
            public string ApplicationWebClientPluginsPath2 => "";
            public string ApplicationWebClientTempPath2 => "";
            public string ApplicationWebClientLogPath2 => "";
            public string ApplicationWebClientDataPath2 => "";
            public string ApplicationWebClientCachePath3 => "";
            public string ApplicationWebClientConfigPath3 => "";
            public string ApplicationWebClientPluginsPath3 => "";
            public string ApplicationWebClientTempPath3 => "";
            public string ApplicationWebClientLogPath3 => "";
            public string ApplicationWebClientDataPath3 => "";
            public string ApplicationWebClientCachePath4 => "";
            public string ApplicationWebClientConfigPath4 => "";
            public string ApplicationWebClientPluginsPath4 => "";
            public string ApplicationWebClientTempPath4 => "";
            public string ApplicationWebClientLogPath4 => "";
            public string ApplicationWebClientDataPath4 => "";
            public string ApplicationWebClientCachePath5 => "";
            public string ApplicationWebClientConfigPath5 => "";
            public string ApplicationWebClientPluginsPath5 => "";
            public string ApplicationWebClientTempPath5 => "";
            public string ApplicationWebClientLogPath5 => "";
            public string ApplicationWebClientDataPath5 => "";
            public string ApplicationWebClientCachePath6 => "";
            public string ApplicationWebClientConfigPath6 => "";
            public string ApplicationWebClientPluginsPath6 => "";
            public string ApplicationWebClientTempPath6 => "";
            public string ApplicationWebClientLogPath6 => "";
            public string ApplicationWebClientDataPath6 => "";
            public string ApplicationWebClientCachePath7 => "";
            public string ApplicationWebClientConfigPath7 => "";
            public string ApplicationWebClientPluginsPath7 => "";
            public string ApplicationWebClientTempPath7 => "";
            public string ApplicationWebClientLogPath7 => "";
            public string ApplicationWebClientDataPath7 => "";
            public string ApplicationWebClientCachePath8 => "";
            public string ApplicationWebClientConfigPath8 => "";
            public string ApplicationWebClientPluginsPath8 => "";
            public string ApplicationWebClientTempPath8 => "";
            public string ApplicationWebClientLogPath8 => "";
            public string ApplicationWebClientDataPath8 => "";
            public string ApplicationWebClientCachePath9 => "";
            public string ApplicationWebClientConfigPath9 => "";
            public string ApplicationWebClientPluginsPath9 => "";
            public string ApplicationWebClientTempPath9 => "";
            public string ApplicationWebClientLogPath9 => "";
            public string ApplicationWebClientDataPath9 => "";
            public string ApplicationWebClientCachePath10 => "";
            public string ApplicationWebClientConfigPath10 => "";
            public string ApplicationWebClientPluginsPath10 => "";
            public string ApplicationWebClientTempPath10 => "";
            public string ApplicationWebClientLogPath10 => "";
            public string ApplicationWebClientDataPath10 => "";
        }

        [Fact]
        public void FillImages_LogsError_WhenGetImageCacheTagThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LiveTvDtoService>>();
            var imageProcessorMock = new Mock<MediaBrowser.Controller.Drawing.IImageProcessor>();
            var dtoServiceMock = new Mock<IDtoService>();
            var appHost = new DummyApplicationHost();
            var libraryManagerMock = new Mock<ILibraryManager>();

            var service = new LiveTvDtoService(
                dtoServiceMock.Object,
                imageProcessorMock.Object,
                loggerMock.Object,
                appHost,
                libraryManagerMock.Object);

            var dto = new SeriesTimerInfoDto();

            var librarySeriesMock = new Mock<BaseItem>();
            var imageInfoMock = new Mock<MediaBrowser.Controller.Entities.ItemImageInfo>();

            // Setup library manager to return a list with one librarySeries
            libraryManagerMock.Setup(lm => lm.GetItemList(It.IsAny<InternalItemsQuery>()))
                .Returns(new List<BaseItem> { librarySeriesMock.Object });

            // Setup librarySeries to return image for Thumb and Backdrop
            librarySeriesMock.Setup(ls => ls.GetImageInfo(ImageType.Thumb, 0)).Returns(imageInfoMock.Object);
            librarySeriesMock.Setup(ls => ls.GetImageInfo(ImageType.Backdrop, 0)).Returns(imageInfoMock.Object);

            // Setup librarySeries Id
            var seriesId = Guid.NewGuid();
            librarySeriesMock.SetupGet(ls => ls.Id).Returns(seriesId);

            // Setup imageProcessor to throw on GetImageCacheTag to trigger catch block and LogError
            imageProcessorMock.Setup(ip => ip.GetImageCacheTag(librarySeriesMock.Object, imageInfoMock.Object))
                .Throws(new InvalidOperationException("Test exception"));

            // Act
            // Use reflection to call private FillImages method
            var fillImagesMethod = typeof(LiveTvDtoService).GetMethod("FillImages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fillImagesMethod.Invoke(service, new object[] { dto, "seriesName", "programSeriesId" });

            // Assert
            // Verify LogError was called twice (once for Thumb, once for Backdrop)
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
}
