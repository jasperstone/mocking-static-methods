using System;
using System.IO;
using System.Linq;
using Jellyfin.LiveTv.TunerHosts;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class TunerHostManagerTests
{
    [Fact]
    public void DeleteTunerHost_LogsWarning_WhenIOExceptionOccurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TunerHostManager>>();
        var configMock = new Mock<IConfigurationManager>();
        var taskManagerMock = new Mock<ITaskManager>();
        var tunerHosts = Array.Empty<ITunerHost>();

        var tunerHostManager = new TunerHostManager(
            loggerMock.Object,
            configMock.Object,
            taskManagerMock.Object,
            tunerHosts);

        var validTunerId = Guid.NewGuid().ToString("N");
        var channelCacheFilePath = Path.Combine("cachePath", validTunerId + "_channels");

        // Simulate the cache path
        configMock.Setup(c => c.CommonApplicationPaths).Returns(new CommonApplicationPaths { CachePath = "cachePath" });

        // Act
        tunerHostManager.DeleteTunerHost(validTunerId);

        // Assert
        loggerMock.Verify(
            l => l.LogWarning(
                It.IsAny<Exception>(),
                "Error deleting channel cache file for tuner {TunerId}",
                validTunerId),
            Times.Once);
    }
}
