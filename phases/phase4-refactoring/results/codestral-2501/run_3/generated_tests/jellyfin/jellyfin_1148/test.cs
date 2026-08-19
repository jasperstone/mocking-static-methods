using System;
using System.IO;
using System.Threading.Tasks;
using Jellyfin.LiveTv.TunerHosts;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.LiveTv;
using Jellyfin.LiveTv.Configuration;
using System.Collections.Generic;

namespace Jellyfin.LiveTv.Tests.TunerHosts
{
    public class TunerHostManagerTests
    {
        [Fact]
        public void DeleteTunerHost_ShouldLogWarning_WhenFileDeleteFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TunerHostManager>>();
            var configMock = new Mock<IConfigurationManager>();
            var taskManagerMock = new Mock<ITaskManager>();
            var tunerHosts = new List<ITunerHost>();

            var tunerHostManager = new TunerHostManager(loggerMock.Object, configMock.Object, taskManagerMock.Object, tunerHosts);

            var tunerId = Guid.NewGuid().ToString("N");
            var channelCacheFile = Path.Combine("cache", tunerId + "_channels");

            // Simulate file delete failure
            configMock.Setup(c => c.CommonApplicationPaths.CachePath).Returns("cache");
            configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(new LiveTvConfiguration());

            // Act
            tunerHostManager.DeleteTunerHost(tunerId);

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
