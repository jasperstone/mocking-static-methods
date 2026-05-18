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
        var mockLogger = new Mock<ILogger<GarnetServerMonitor>>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockGarnetServerOptions = new Mock<GarnetServerOptions>();
        var mockGarnetServer = new Mock<IGarnetServer>();
        var mockRespServerSession = new Mock<RespServerSession>();

        var servers = new[] { mockGarnetServer.Object };
        var resetEventFlags = new Dictionary<InfoMetricsType, bool>
        {
            { InfoMetricsType.COMMANDSTATS, true }
        };

        var globalMetrics = new GarnetServerMetrics(true, true, true, null);

        var monitor = new GarnetServerMonitor(
            mockStoreWrapper.Object,
            mockGarnetServerOptions.Object,
            servers,
            mockLogger.Object
        );

        // Act
        monitor.CleanupGlobalStats();

        // Assert
        mockLogger.Verify(
            logger => logger.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
