using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

public class GarnetServerMonitorTests
{
    [Fact]
    public void LogInformation_ShouldBeCalled_WhenResettingCommandStats()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        var garnetServerOptions = new GarnetServerOptions();
        var servers = new IGarnetServer[0];

        var monitor = new GarnetServerMonitor(storeWrapperMock.Object, garnetServerOptions, servers, loggerMock.Object);

        monitor.resetEventFlags[GarnetInfoMetrics.InfoMetricsType.COMMANDSTATS] = true;

        // Act
        monitor.CleanupGlobalStats();

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Resetting command stats")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
