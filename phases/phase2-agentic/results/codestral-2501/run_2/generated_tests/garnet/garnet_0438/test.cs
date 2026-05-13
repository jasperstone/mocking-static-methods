using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

namespace Garnet.Tests
{
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
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Resetting command stats")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
