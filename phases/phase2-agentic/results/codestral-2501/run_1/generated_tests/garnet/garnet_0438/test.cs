using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

namespace Garnet.Tests
{
    public class GarnetServerMonitorTests
    {
        [Fact]
        public void LogInformation_Called_When_Resetting_Command_Stats()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<GarnetServerMonitor>>();
            var monitor = new GarnetServerMonitor(null, new GarnetServerOptions(), new IGarnetServer[0], mockLogger.Object);
            monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;

            // Act
            monitor.CleanupGlobalStats();

            // Assert
            mockLogger.Verify(
                x => x.LogInformation("Resetting command stats"),
                Times.Once());
        }
    }
}
