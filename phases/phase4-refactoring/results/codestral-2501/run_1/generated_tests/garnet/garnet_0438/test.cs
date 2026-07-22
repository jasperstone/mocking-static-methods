using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

public class GarnetServerMonitorTests
{
    [Fact]
    public void LogInformation_Called_When_ResettingCommandStats()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GarnetServerMonitor>>();
        var storeWrapper = new StoreWrapper();
        var opts = new GarnetServerOptions();
        var servers = new IGarnetServer[0];
        var monitor = new GarnetServerMonitor(storeWrapper, opts, servers, mockLogger.Object);

        // Act
        monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;
        monitor.CleanupGlobalStats();

        // Assert
        mockLogger.Verify(
            x => x.LogInformation(
                "Resetting command stats",
                It.IsAny<object[]>(),
                It.IsAny<System.Exception>(),
                It.IsAny<Func<object, Exception, string>>()),
            Times.Once);
    }
}
