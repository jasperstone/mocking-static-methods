using Xunit;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.TunerHosts;
using Microsoft.Extensions.Logging;

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
            var taskManagerMock = new Mock<ITaskManager>();
            var tunerHosts = new ITunerHost[0];

            var tunerHostManager = new TunerHostManager(loggerMock.Object, configMock.Object, taskManagerMock.Object, tunerHosts);

            var id = Guid.NewGuid().ToString("N");
            var safeId = id;
            var channelCacheFile = Path.GetTempFileName();

            configMock.Setup(c => c.CommonApplicationPaths.CachePath).Returns(Path.GetDirectoryName(channelCacheFile));

            // Act
            try
            {
                File.Create(channelCacheFile).Dispose();
                File.SetAttributes(channelCacheFile, FileAttributes.ReadOnly);

                tunerHostManager.DeleteTunerHost(id);

                // Assert
                loggerMock.Verify(l => l.LogWarning(It.IsAny<IOException>(), "Error deleting channel cache file for tuner {TunerId}", safeId), Times.Once);
            }
            finally
            {
                File.SetAttributes(channelCacheFile, FileAttributes.Normal);
                File.Delete(channelCacheFile);
            }
        }
    }
}
