using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.TunerHosts.Tests
{
    public class TunerHostManagerTests
    {
        [Fact]
        public void DeleteTunerHost_LogsWarning_WhenIOExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TunerHostManager>>();
            var configMock = new Mock<IConfigurationManager>();
            var taskManagerMock = new Mock<ITaskManager>();

            var tunerHostManager = new TunerHostManager(
                loggerMock.Object,
                configMock.Object,
                taskManagerMock.Object,
                Array.Empty<ITunerHost>());

            var tunerId = Guid.NewGuid().ToString("N");
            var channelCacheFilePath = Path.Combine("cachePath", tunerId + "_channels");

            // Simulate the configuration and cache path
            configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(new LiveTvConfiguration
            {
                TunerHosts = Array.Empty<TunerHostInfo>()
            });
            configMock.Setup(c => c.CommonApplicationPaths).Returns(new CommonApplicationPaths
            {
                CachePath = "cachePath"
            });

            // Act
            var exception = new IOException("Test exception");
            System.IO.File.SetAttributes(channelCacheFilePath, FileAttributes.ReadOnly); // Simulate read-only file

            try
            {
                tunerHostManager.DeleteTunerHost(tunerId);
            }
            finally
            {
                System.IO.File.SetAttributes(channelCacheFilePath, FileAttributes.Normal); // Clean up
            }

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(It.IsAny<Exception>(), "Error deleting channel cache file for tuner {TunerId}", tunerId),
                Times.Once);
        }
    }
}
