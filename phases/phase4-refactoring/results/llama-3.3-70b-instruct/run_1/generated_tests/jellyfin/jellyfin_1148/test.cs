using Xunit;
using Moq;
using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.LiveTv;
using Jellyfin.LiveTv.TunerHosts;

namespace Jellyfin.LiveTv.TunerHosts
{
    public class TunerHostManagerTests
    {
        [Fact]
        public void DeleteTunerHost_LogsWarningOnIOException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TunerHostManager>>();
            var configMock = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            var taskManagerMock = new Mock<MediaBrowser.Controller.LiveTv.ITaskManager>();
            var tunerHostMock = new Mock<MediaBrowser.Controller.LiveTv.ITunerHost>();
            var tunerHosts = new[] { tunerHostMock.Object };
            var tunerHostManager = new TunerHostManager(loggerMock.Object, configMock.Object, taskManagerMock.Object, tunerHosts);

            var id = Guid.NewGuid().ToString("N");
            var safeId = id;
            var channelCacheFile = Path.Combine("cache", safeId + "_channels");
            File.Create(channelCacheFile).Dispose();

            // Act and Assert
            loggerMock.Setup(l => l.LogWarning(It.IsAny<IOException>(), It.IsAny<string>(), It.IsAny<object>())).Verifiable();
            tunerHostManager.DeleteTunerHost(id);
            loggerMock.Verify(l => l.LogWarning(It.IsAny<IOException>(), It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            File.Delete(channelCacheFile);
        }
    }
}
