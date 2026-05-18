using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using System.Collections.Generic;
using Garnet.server; // Assuming this is the correct namespace for InfoMetricsType

namespace Garnet.server.Tests
{
    // Partial class to expose GarnetServerMonitor for testing
    public partial class GarnetServerMonitor
    {
        public void TestCleanupGlobalStats()
        {
            CleanupGlobalStats();
        }
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

            // Use a partial class or a wrapper to expose the constructor and fields
            var monitor = new GarnetServerMonitor(storeWrapperMock.Object, optsMock.Object, serversMock, loggerMock.Object)
            {
                resetEventFlags = resetEventFlags
            };

            // Act
            monitor.TestCleanupGlobalStats();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Resetting command stats"),
                Times.Once);
        }
    }
}
