using Xunit;
using Moq;
using System;
using System.IO;
using System.Threading;
using Jellyfin.LiveTv.TunerHosts;
using Microsoft.Extensions.Logging;

namespace Jellyfin.LiveTv.Tests
{
    public class TunerHostManagerTests
    {
        [Fact]
        public void DeleteTunerHost_LogsWarningOnIOException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TunerHostManager>>();
            var configMock = new Mock<IConfigurationManager>();
            var taskManagerMock = new Mock<ITaskManager>();
            var tunerHosts = new ITunerHost[0];

            var tunerHostManager = new TunerHostManager(loggerMock.Object, configMock.Object, taskManagerMock.Object, tunerHosts);

            var id = Guid.NewGuid().ToString("N");
            var channelCacheFile = Path.Combine("cache", id + "_channels");

            // Make the file exist so we can test the delete
            File.Create(channelCacheFile).Dispose();

            // Make the delete fail
            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(f => f.File.Delete(It.IsAny<string>())).Throws(new IOException());

            // Act
            tunerHostManager.DeleteTunerHost(id);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<IOException>(), "Error deleting channel cache file for tuner {TunerId}", id), Times.Once);
        }
    }
}
