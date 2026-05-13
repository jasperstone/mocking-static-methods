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
        var logger = new Mock<ILogger<GarnetServerMonitor>>();
        var mockServers = Array.Empty<IGarnetServer>();
        var storeWrapper = new Mock<StoreWrapper>();
        var opts = new GarnetServerOptions { CommandStatsMonitor = true, MetricsSamplingFrequency = 1 };

        var monitor = new GarnetServerMonitor(storeWrapper.Object, opts, mockServers, logger.Object);
        monitor.resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;

        // Act
        monitor.GetType().GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(monitor, null);

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("Resetting command stats")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void CleanupGlobalStats_ResetsStats_LogsInformationMessage()
    {
        // Arrange
        var logger = new Mock<ILogger<GarnetServerMonitor>>();
        var mockServers = Array.Empty<IGarnetServer>();
        var storeWrapper = new Mock<StoreWrapper>();
        var opts = new GarnetServerOptions { CommandStatsMonitor = true, MetricsSamplingFrequency = 1 };

        var monitor = new GarnetServerMonitor(storeWrapper.Object, opts, mockServers, logger.Object);
        monitor.resetEventFlags[InfoMetricsType.STATS] = true;

        // Act
        monitor.GetType().GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(monitor, null);

        // Assert - Verifies the LogInformation call on line ~200 for STATS reset
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("Resetting latency metrics for commands")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void CleanupGlobalLatencyMetrics_ResetsLatencyMetrics_LogsInformationMessage()
    {
        // Arrange
        var logger = new Mock<ILogger<GarnetServerMonitor>>();
        var mockServers = Array.Empty<IGarnetServer>();
        var storeWrapper = new Mock<StoreWrapper>();
        var opts = new GarnetServerOptions { LatencyMonitor = true, MetricsSamplingFrequency = 1 };

        var monitor = new GarnetServerMonitor(storeWrapper.Object, opts, mockServers, logger.Object);
        monitor.resetLatencyMetrics[LatencyMetricsType.COMMAND] = true;

        // Act
        monitor.GetType().GetMethod("CleanupGlobalLatencyMetrics", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(monitor, null);

        // Assert - Verifies the LogInformation call shown in the snippet for latency metrics
        logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("Resetting server-side stats") && v.ToString().Contains("COMMAND")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void CleanupGlobalStats_NoResetFlags_NoLogCall()
    {
        // Arrange
        var logger = new Mock<ILogger<GarnetServerMonitor>>();
        var mockServers = Array.Empty<IGarnetServer>();
        var storeWrapper = new Mock<StoreWrapper>();
        var opts = new GarnetServerOptions { MetricsSamplingFrequency = 1 };

        var monitor = new GarnetServerMonitor(storeWrapper.Object, opts, mockServers, logger.Object);

        // Act
        monitor.GetType().GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(monitor, null);

        // Assert - No logging when no reset flags are set
        logger.VerifyNoOtherCalls();
    }
}
