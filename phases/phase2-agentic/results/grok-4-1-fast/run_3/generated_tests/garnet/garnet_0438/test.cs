using System;
using System.Collections.Generic;
using Garnet.common;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class GarnetServerMonitorTests
{
    [Fact]
    public void CleanupGlobalStats_ResetsCommandStats_LogsInformationMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GarnetServerMonitor>>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockServers = Array.Empty<IGarnetServer>();
        var opts = new GarnetServerOptions { MetricsSamplingFrequency = 1, CommandStatsMonitor = true };

        var monitor = new GarnetServerMonitor(mockStoreWrapper.Object, opts, mockServers, mockLogger.Object);
        monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;

        // Act
        monitor.GetType()
            .GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(monitor, null);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString() == "Resetting command stats"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
            ),
            Times.Once
        );
    }

    [Fact]
    public void CleanupGlobalStats_ResetsStats_LogsInformationMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GarnetServerMonitor>>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockServers = Array.Empty<IGarnetServer>();
        var opts = new GarnetServerOptions { MetricsSamplingFrequency = 1 };

        var monitor = new GarnetServerMonitor(mockStoreWrapper.Object, opts, mockServers, mockLogger.Object);
        monitor.resetEventFlags[InfoMetricsType.STATS] = true;

        // Act
        monitor.GetType()
            .GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(monitor, null);

        // Assert - logs "Resetting latency metrics for commands" (line ~188 context)
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString() == "Resetting latency metrics for commands"),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
            ),
            Times.Once
        );
    }

    [Fact]
    public void CleanupGlobalLatencyMetrics_ResetsLatencyMetrics_LogsInformationMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GarnetServerMonitor>>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockServers = Array.Empty<IGarnetServer>();
        var opts = new GarnetServerOptions { MetricsSamplingFrequency = 1, LatencyMonitor = true };

        var monitor = new GarnetServerMonitor(mockStoreWrapper.Object, opts, mockServers, mockLogger.Object);
        monitor.resetLatencyMetrics[LatencyMetricsType.COMMAND] = true;

        // Act
        monitor.GetType()
            .GetMethod("CleanupGlobalLatencyMetrics", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(monitor, null);

        // Assert - logs "Resetting server-side stats {eventType}"
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("Resetting server-side stats COMMAND")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
            ),
            Times.Once
        );
    }

    [Fact]
    public void CleanupGlobalStats_CommandStatsResetFlagFalse_DoesNotLog()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GarnetServerMonitor>>();
        var mockStoreWrapper = new Mock<StoreWrapper>();
        var mockServers = Array.Empty<IGarnetServer>();
        var opts = new GarnetServerOptions { MetricsSamplingFrequency = 1 };

        var monitor = new GarnetServerMonitor(mockStoreWrapper.Object, opts, mockServers, mockLogger.Object);
        // Leave resetEventFlags[COMMANDSTATS] as false (default)

        // Act
        monitor.GetType()
            .GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(monitor, null);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString() == "Resetting command stats"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Never
        );
    }
}
