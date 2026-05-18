using Xunit;
using Moq;
using System;
using System.IO;
using System.Threading;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.LiveTv;
using Microsoft.Extensions.Logging;

namespace Jellyfin.LiveTv.TunerHosts.Tests
{
    public class TunerHostManagerTests
    {
        [Fact]
        public void DeleteTunerHost_LogsWarningWhenDeletingChannelCacheFileFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TunerHostManager>>();
            var configMock = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            var taskManagerMock = new Mock<MediaBrowser.Controller.Tasks.ITaskManager>();
            var tunerHostsMock = new Mock<System.Collections.Generic.IEnumerable<MediaBrowser.Controller.LiveTv.ITunerHost>>();

            var tunerHostManager = new TunerHostManager(loggerMock.Object, configMock.Object, taskManagerMock.Object, tunerHostsMock.Object);

            var id = Guid.NewGuid().ToString("N");
            var safeId = id;
            var channelCacheFile = Path.Combine("cache", safeId + "_channels");

            // Make the file deletion fail
            using (File.Create(channelCacheFile)) { }

            // Act
            tunerHostManager.DeleteTunerHost(id);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<IOException>(), "Error deleting channel cache file for tuner {TunerId}", safeId), Times.Once);
        }
    }
}
