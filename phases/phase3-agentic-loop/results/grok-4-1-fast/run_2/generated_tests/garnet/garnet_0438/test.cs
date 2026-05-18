using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Garnet.server;

namespace Garnet.server.Tests
{
    public class GarnetServerMonitorLoggerTests
    {
        [Fact]
        public void CleanupGlobalStats_WhenCommandStatsResetFlagIsTrue_LogsResettingCommandStatsMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GarnetServerMonitor>>();
            loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            
            var storeWrapperMock = new Mock<StoreWrapper>("", "", Array.Empty<IGarnetServer>(), 
                new Mock<CustomCommandManager>().Object, new GarnetServerOptions(), new Mock<SubscribeBroker>().Object());
            
            var optionsMock = new Mock<GarnetServerOptions>();
            optionsMock.Setup(o => o.CommandStatsMonitor).Returns(true);
            optionsMock.Setup(o => o.MetricsSamplingFrequency).Returns(1);
            
            var monitorType = typeof(GarnetServerMonitor);
            var monitor = (GarnetServerMonitor)Activator.CreateInstance(monitorType, 
                storeWrapperMock.Object, optionsMock.Object, 
                Array.Empty<IGarnetServer>(), loggerMock.Object);

            var resetFlagsField = monitorType.GetField("resetEventFlags", 
                BindingFlags.Public | BindingFlags.Instance);
            var resetFlags = (Dictionary<InfoMetricsType, bool>)resetFlagsField.GetValue(monitor);
            resetFlags[InfoMetricsType.COMMANDSTATS] = true;

            // Act
            var cleanupMethod = monitorType.GetMethod("CleanupGlobalStats", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            cleanupMethod.Invoke(monitor, null);

            // Assert - verify LogInformation extension was called
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Resetting command stats")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupGlobalStats_WhenStatsResetFlagIsTrue_LogsResettingLatencyMetricsMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GarnetServerMonitor>>();
            loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            
            var storeWrapperMock = new Mock<StoreWrapper>("", "", Array.Empty<IGarnetServer>(), 
                new Mock<CustomCommandManager>().Object, new GarnetServerOptions(), new Mock<SubscribeBroker>().Object());
            
            var optionsMock = new Mock<GarnetServerOptions>();
            optionsMock.Setup(o => o.LatencyMonitor).Returns(true);
            
            var monitorType = typeof(GarnetServerMonitor);
            var monitor = (GarnetServerMonitor)Activator.CreateInstance(monitorType, 
                storeWrapperMock.Object, optionsMock.Object, 
                Array.Empty<IGarnetServer>(), loggerMock.Object);

            var resetFlagsField = monitorType.GetField("resetEventFlags", 
                BindingFlags.Public | BindingFlags.Instance);
            var resetFlags = (Dictionary<InfoMetricsType, bool>)resetFlagsField.GetValue(monitor);
            resetFlags[InfoMetricsType.STATS] = true;

            // Act
            var cleanupMethod = monitorType.GetMethod("CleanupGlobalStats", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            cleanupMethod.Invoke(monitor, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Resetting latency metrics for commands")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
