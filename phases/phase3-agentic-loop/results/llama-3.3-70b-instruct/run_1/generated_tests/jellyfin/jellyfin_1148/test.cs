using Xunit;
using Moq;
using System;
using System.IO;
using System.Globalization;
using System.Threading;
using Jellyfin.LiveTv.TunerHosts;
using MediaBrowser.Common.Configuration;
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
            var tunerHosts = new ITunerHost[0];

            var tunerHostManager = new TunerHostManager(loggerMock.Object, configMock.Object, null, tunerHosts);

            var id = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            var channelCacheFile = Path.GetTempFileName();

            // Act and Assert
            loggerMock.Setup(l => l.LogWarning(It.IsAny<IOException>(), It.IsAny<string>(), It.IsAny<object>()));
            tunerHostManager.DeleteTunerHost(id);

            // Verify
            loggerMock.Verify(l => l.LogWarning(It.IsAny<IOException>(), It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void DeleteTunerHost_DoesNotLogWarningOnSuccessfulDelete()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TunerHostManager>>();
            var configMock = new Mock<IConfigurationManager>();
            var tunerHosts = new ITunerHost[0];

            var tunerHostManager = new TunerHostManager(loggerMock.Object, configMock.Object, null, tunerHosts);

            var id = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            var channelCacheFile = Path.GetTempFileName();
            File.Create(channelCacheFile).Dispose();

            // Act and Assert
            loggerMock.Setup(l => l.LogWarning(It.IsAny<IOException>(), It.IsAny<string>(), It.IsAny<object>()));
            tunerHostManager.DeleteTunerHost(id);

            // Verify
            loggerMock.Verify(l => l.LogWarning(It.IsAny<IOException>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}
