using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server;
using System.Collections.Generic;

public class GarnetServerMonitorTests
{
    [Fact]
    public void LogInformation_Called_When_ResettingCommandStats()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockGarnetServerOptions = new Mock<GarnetServerOptions>();
        var mockGarnetServers = new List<IGarnetServer>();

        var monitor = new GarnetServerMonitor(mockStoreWrapper.Object, mockGarnetServerOptions.Object, mockGarnetServers.ToArray(), mockLogger.Object);

        // Act
        monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;
        monitor.CleanupGlobalStats();

        // Assert
        mockLogger.Verify(logger => logger.LogInformation("Resetting command stats"), Times.Once);
    }
}
