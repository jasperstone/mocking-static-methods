using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.TunerHosts;
using Jellyfin.LiveTv.Configuration;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Tasks;
using System.Threading.Tasks;

namespace Jellyfin.LiveTv.Tests.TunerHosts
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
            var tunerHostMock = new Mock<ITunerHost>();
            tunerHostMock.SetupGet(t => t.IsSupported).Returns(true);

            var liveTvConfig = new LiveTvConfiguration
            {
                TunerHosts = new[]
                {
                    new TunerHostInfo { Id = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture) }
                }
            };

            configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(liveTvConfig);
            configMock.SetupGet(c => c.CommonApplicationPaths).Returns(new CommonApplicationPaths { CachePath = Path.GetTempPath() });

            var manager = new TunerHostManager(
                loggerMock.Object,
                configMock.Object,
                taskManagerMock.Object,
                new[] { tunerHostMock.Object });

            // Use a valid GUID id to pass the Guid.TryParseExact check
            var id = liveTvConfig.TunerHosts[0].Id;

            // Setup File.Delete to throw IOException
            var channelCacheFile = Path.Combine(configMock.Object.CommonApplicationPaths.CachePath, id + "_channels");

            // We cannot mock static File.Delete directly, so we simulate by creating a file and locking it to cause IOException
            // Create the file
            File.WriteAllText(channelCacheFile, "test");
            // Open the file to lock it
            using (var stream = File.Open(channelCacheFile, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // Act
                manager.DeleteTunerHost(id);
            }
            // Cleanup the file after test
            if (File.Exists(channelCacheFile))
            {
                File.Delete(channelCacheFile);
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting channel cache file for tuner")),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
