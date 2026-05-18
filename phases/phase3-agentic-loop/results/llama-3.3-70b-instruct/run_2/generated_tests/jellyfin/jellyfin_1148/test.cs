using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.TunerHosts;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests
{
    public class TunerHostManagerTests
    {
        private readonly Mock<ILogger<TunerHostManager>> _loggerMock;
        private readonly Mock<IConfigurationManager> _configMock;

        public TunerHostManagerTests()
        {
            _loggerMock = new Mock<ILogger<TunerHostManager>>();
            _configMock = new Mock<IConfigurationManager>();
        }

        [Fact]
        public async Task DeleteTunerHost_LogsWarning_WhenDeletingChannelCacheFileFails()
        {
            // Arrange
            var tunerHostManager = new TunerHostManager(_loggerMock.Object, _configMock.Object, null, null);
            var id = Guid.NewGuid().ToString("N");
            var safeId = id;
            var channelCacheFile = Path.Combine("cache", safeId + "_channels");
            _configMock.Setup(c => c.CommonApplicationPaths).Returns(new CommonApplicationPaths { CachePath = "cache" });
            _configMock.Setup(c => c.GetConfiguration(It.IsAny<string>())).Returns(new LiveTvConfiguration { TunerHosts = new[] { new TunerHostInfo { Id = id } } });
            _configMock.Setup(c => c.SaveConfiguration(It.IsAny<string>(), It.IsAny<object>())).Verifiable();

            // Act
            tunerHostManager.DeleteTunerHost(id);

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.IsAny<IOException>(), "Error deleting channel cache file for tuner {TunerId}", safeId), Times.Once);
        }
    }
}
