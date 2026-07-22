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
        var mockLogger = new Mock<ILogger>();
        var monitor = new GarnetServerMonitor(null, new GarnetServerOptions(), new IGarnetServer[0], mockLogger.Object);

        // Act
        monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;
        monitor.CleanupGlobalStats();

        // Assert
        mockLogger.Verify(
            logger => logger.LogInformation("Resetting command stats"),
            Times.Once()
        );
    }
}
