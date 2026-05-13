using Xunit;
using Moq;
using System;
using System.IO;
using System.Globalization;
using System.Threading;
using Jellyfin.LiveTv.TunerHosts;
using Microsoft.Extensions.Logging;

namespace Jellyfin.LiveTv.Tests
{
    public class TunerHostManagerTests
    {
        private readonly Mock<ILogger<TunerHostManager>> _loggerMock;
        private readonly Mock<IConfigurationManager> _configMock;
        private readonly Mock<ITaskManager> _taskManagerMock;
        private readonly Mock<ITunerHost> _tunerHostMock;

        public TunerHostManagerTests()
        {
            _loggerMock = new Mock<ILogger<TunerHostManager>>();
            _configMock = new Mock<IConfigurationManager>();
            _taskManagerMock = new Mock<ITaskManager>();
            _tunerHostMock = new Mock<ITunerHost>();
        }

        [Fact]
        public void DeleteTunerHost_LogsWarningWhenDeletingChannelCacheFileFails()
        {
            // Arrange
            var tunerHostManager = new TunerHostManager(_loggerMock.Object, _configMock.Object, _taskManagerMock.Object, new[] { _tunerHostMock.Object });
            var id = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            var channelCacheFile = Path.Combine("cache", id + "_channels");
            var ex = new IOException("Test exception");

            // Act
            tunerHostManager.DeleteTunerHost(id);

            // Assert
            _loggerMock.Verify(logger => logger.LogWarning(It.IsAny<IOException>(), "Error deleting channel cache file for tuner {TunerId}", id), Times.Once);
        }
    }
}
