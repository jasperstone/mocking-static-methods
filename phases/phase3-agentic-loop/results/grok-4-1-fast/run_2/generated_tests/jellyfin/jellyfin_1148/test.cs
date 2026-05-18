using System;
using System.Globalization;
using System.IO;
using Jellyfin.LiveTv.TunerHosts;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.LiveTv.Tests.TunerHosts
{
    public class TunerHostManagerTests
    {
        private const string ValidTunerId = "0000000000000000000000000000000000000000000000000000000000000000";

        [Fact]
        public void DeleteTunerHost_ValidGuid_FileDeleteThrowsIOException_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<TunerHostManager>>();
            var mockConfig = new Mock<IConfigurationManager>();
            var mockTaskManager = new Mock<ITaskManager>();
            var tunerHosts = Array.Empty<ITunerHost>();

            // Mock the configuration with extension method support
            var liveTvConfig = new { TunerHosts = Array.Empty<TunerHostInfo>() };
            mockConfig.Setup(c => c.GetLiveTvConfiguration()).Returns(liveTvConfig);

            // Mock CommonApplicationPaths via setup
            mockConfig.Setup(c => c.CommonApplicationPaths)
                .Returns(new Mock<ICommonApplicationPaths>().Object);

            var manager = new TunerHostManager(mockLogger.Object, mockConfig.Object, mockTaskManager.Object, tunerHosts);

            // Setup logger to verify the exact LogWarning call on line 123
            mockLogger.Setup(l => l.LogWarning(
                It.IsAny<IOException>(),
                "Error deleting channel cache file for tuner {TunerId}",
                ValidTunerId));

            // Act
            manager.DeleteTunerHost(ValidTunerId);

            // Assert - verify the LogWarning extension was called
            mockLogger.Verify(
                l => l.LogWarning(
                    It.IsAny<IOException>(),
                    "Error deleting channel cache file for tuner {TunerId}",
                    ValidTunerId),
                Times.Once);
        }
    }
}
