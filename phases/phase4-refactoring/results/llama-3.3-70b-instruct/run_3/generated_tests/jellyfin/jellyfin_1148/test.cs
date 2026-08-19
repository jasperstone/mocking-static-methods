using Jellyfin.LiveTv.TunerHosts;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Jellyfin.LiveTv.Tests
{
    public class TunerHostManagerTests
    {
        [Fact]
        public async Task DeleteTunerHost_LogsWarning_WhenDeletingChannelCacheFileFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TunerHostManager>>();
            var configMock = new Mock<IConfigurationManager>();
            configMock.Setup(c => c.CommonApplicationPaths.CachePath).Returns("cache");
            var taskManagerMock = new Mock<ITaskManager>();
            var tunerHosts = new MediaBrowser.Controller.LiveTv.ITunerHost[0];

            var tunerHostManager = new TunerHostManager(loggerMock.Object, configMock.Object, taskManagerMock.Object, tunerHosts);

            var id = Guid.NewGuid().ToString("N");
            var channelCacheFile = Path.Combine("cache", id + "_channels");
            using var _ = File.Create(channelCacheFile);

            // Act
            tunerHostManager.DeleteTunerHost(id);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<IOException>(), "Error deleting channel cache file for tuner {TunerId}", id), Times.Never);

            File.Delete(channelCacheFile);
        }

        [Fact]
        public async Task DeleteTunerHost_LogsWarning_WhenDeletingChannelCacheFileFailsDueToIOException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TunerHostManager>>();
            var configMock = new Mock<IConfigurationManager>();
            configMock.Setup(c => c.CommonApplicationPaths.CachePath).Returns("cache");
            var taskManagerMock = new Mock<ITaskManager>();
            var tunerHosts = new MediaBrowser.Controller.LiveTv.ITunerHost[0];

            var tunerHostManager = new TunerHostManager(loggerMock.Object, configMock.Object, taskManagerMock.Object, tunerHosts);

            var id = Guid.NewGuid().ToString("N");
            var channelCacheFile = Path.Combine("cache", id + "_channels");
            using var _ = File.Create(channelCacheFile);

            using (File.Open(channelCacheFile, FileMode.Open, FileAccess.Write, FileShare.None))
            {
                // Act
                tunerHostManager.DeleteTunerHost(id);

                // Assert
                loggerMock.Verify(l => l.LogWarning(It.IsAny<IOException>(), "Error deleting channel cache file for tuner {TunerId}", id), Times.Once);
            }

            File.Delete(channelCacheFile);
        }
    }
}
