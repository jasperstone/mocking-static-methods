using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.TunerHosts;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Tasks;

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
                TunerHosts = Array.Empty<TunerHostInfo>()
            };
            configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(liveTvConfig);
            configMock.SetupGet(c => c.CommonApplicationPaths).Returns(new CommonApplicationPaths { CachePath = Path.GetTempPath() });
            configMock.Setup(c => c.SaveConfiguration(It.IsAny<string>(), It.IsAny<LiveTvConfiguration>()));

            var manager = new TestableTunerHostManager(loggerMock.Object, configMock.Object, taskManagerMock.Object, new[] { tunerHostMock.Object });

            // Create a valid GUID string in "N" format
            var tunerId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            var channelCacheFile = Path.Combine(configMock.Object.CommonApplicationPaths.CachePath, tunerId + "_channels");

            // Setup File.Delete to throw IOException
            TestableTunerHostManager.FileDeleteOverride = path =>
            {
                if (path == channelCacheFile)
                {
                    throw new IOException("Test IOException");
                }
                else
                {
                    File.Delete(path);
                }
            };

            // Act
            manager.DeleteTunerHost(tunerId);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting channel cache file for tuner")),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Cleanup override
            TestableTunerHostManager.FileDeleteOverride = null;
        }

        private class TestableTunerHostManager : TunerHostManager
        {
            public static Action<string>? FileDeleteOverride;

            public TestableTunerHostManager(ILogger<TunerHostManager> logger, IConfigurationManager config, ITaskManager taskManager, ITunerHost[] tunerHosts)
                : base(logger, config, taskManager, tunerHosts)
            {
            }

            public new void DeleteTunerHost(string? id)
            {
                var config = _config.GetLiveTvConfiguration();
                config.TunerHosts = config.TunerHosts.Where(i => !string.Equals(id, i.Id, StringComparison.OrdinalIgnoreCase)).ToArray();
                _config.SaveConfiguration("livetv", config);

                if (Guid.TryParseExact(id, "N", out var tunerGuid))
                {
                    var safeId = tunerGuid.ToString("N", CultureInfo.InvariantCulture);
                    var channelCacheFile = Path.Combine(_config.CommonApplicationPaths.CachePath, safeId + "_channels");
                    try
                    {
                        if (FileDeleteOverride != null)
                        {
                            FileDeleteOverride(channelCacheFile);
                        }
                        else
                        {
                            File.Delete(channelCacheFile);
                        }
                    }
                    catch (IOException ex)
                    {
                        _logger.LogWarning(ex, "Error deleting channel cache file for tuner {TunerId}", safeId);
                    }
                }

                _taskManager.CancelIfRunningAndQueue<RefreshGuideScheduledTask>();
            }
        }
    }
}
