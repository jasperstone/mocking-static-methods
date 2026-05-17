using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using System.Collections.Generic;

namespace Garnet.server.Tests
{
    // Assuming InfoMetricsType is an enum
    public enum InfoMetricsType
    {
        COMMANDSTATS,
        STATS
    }

    public class GarnetServerMonitorTests
    {
        [Fact]
        public void Should_Log_Information_When_Resetting_Command_Stats()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var optsMock = new Mock<GarnetServerOptions>();
            var serversMock = new IGarnetServer[0]; // Assuming no servers for simplicity

            var resetEventFlags = new Dictionary<InfoMetricsType, bool>
            {
                { InfoMetricsType.COMMANDSTATS, true }
            };

            // Accessing GarnetServerMonitor as it is now accessible
            var monitor = new GarnetServerMonitor(storeWrapperMock.Object, optsMock.Object, serversMock, loggerMock.Object)
            {
                resetEventFlags = resetEventFlags
            };

            // Act
            monitor.CleanupGlobalStats();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Resetting command stats"),
                Times.Once);
        }
    }
}
