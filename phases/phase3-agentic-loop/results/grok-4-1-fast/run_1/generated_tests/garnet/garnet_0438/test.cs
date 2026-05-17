using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Microsoft.Extensions.Logging;
using Garnet.common;

namespace Garnet.server.Metrics.Tests
{
    public class GarnetServerMonitorLoggerTests
    {
        [Fact]
        public void CleanupGlobalStats_LogsResettingCommandStats_WhenCommandStatsFlagSet()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GarnetServerMonitor>>();
            var serversMock = new Mock<IGarnetServer>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            
            // Create monitor using reflection (internal class)
            var monitorType = Type.GetType("Garnet.server.GarnetServerMonitor, Garnet.server")!;
            var monitor = (dynamic)Activator.CreateInstance(
                monitorType, storeWrapperMock.Object, new { LatencyMonitor = false }, 
                new[] { serversMock.Object }, loggerMock.Object)!;

            // Set internal reset flag using reflection
            var resetFlagsField = monitorType.GetField("resetEventFlags", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            resetFlagsField.SetValue(monitor, new Dictionary<InfoMetricsType, bool> 
            { 
                [InfoMetricsType.COMMANDSTATS] = true 
            });

            // Act - invoke private method using reflection
            var cleanupMethod = monitorType.GetMethod("CleanupGlobalStats", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            cleanupMethod.Invoke(monitor, null);

            // Assert - verify LogInformation was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t?.ToString().Contains("Resetting command stats") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupGlobalStats_LogsResettingStats_WhenStatsFlagSet()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GarnetServerMonitor>>();
            var serversMock = new Mock<IGarnetServer>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            
            var monitorType = Type.GetType("Garnet.server.GarnetServerMonitor, Garnet.server")!;
            var monitor = (dynamic)Activator.CreateInstance(
                monitorType, storeWrapperMock.Object, new { LatencyMonitor = false }, 
                new[] { serversMock.Object }, loggerMock.Object)!;

            var resetFlagsField = monitorType.GetField("resetEventFlags", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            resetFlagsField.SetValue(monitor, new Dictionary<InfoMetricsType, bool> 
            { 
                [InfoMetricsType.STATS] = true 
            });

            // Act
            var cleanupMethod = monitorType.GetMethod("CleanupGlobalStats", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            cleanupMethod.Invoke(monitor, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t?.ToString().Contains("Resetting latency metrics for commands") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupGlobalLatencyMetrics_LogsResettingServerSideStats()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<GarnetServerMonitor>>();
            var serversMock = new Mock<IGarnetServer>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            
            var monitorType = Type.GetType("Garnet.server.GarnetServerMonitor, Garnet.server")!;
            var opts = new { LatencyMonitor = true };
            var monitor = (dynamic)Activator.CreateInstance(
                monitorType, storeWrapperMock.Object, opts, new[] { serversMock.Object }, loggerMock.Object)!;

            var resetLatencyField = monitorType.GetField("resetLatencyMetrics", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            resetLatencyField.SetValue(monitor, GarnetLatencyMetrics.defaultLatencyTypes.ToDictionary(x => x, y => true));

            // Act
            var cleanupLatencyMethod = monitorType.GetMethod("CleanupGlobalLatencyMetrics", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            cleanupLatencyMethod.Invoke(monitor, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t?.ToString().Contains("Resetting server-side stats") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void LogInformationExtension_NullLogger_DoesNotThrow()
        {
            // Arrange
            var serversMock = new Mock<IGarnetServer>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            
            var monitorType = Type.GetType("Garnet.server.GarnetServerMonitor, Garnet.server")!;
            var monitor = (dynamic)Activator.CreateInstance(
                monitorType, storeWrapperMock.Object, new { LatencyMonitor = false }, 
                new[] { serversMock.Object }, null)!;

            var resetFlagsField = monitorType.GetField("resetEventFlags", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            resetFlagsField.SetValue(monitor, new Dictionary<InfoMetricsType, bool> 
            { 
                [InfoMetricsType.COMMANDSTATS] = true 
            });

            // Act & Assert
            var cleanupMethod = monitorType.GetMethod("CleanupGlobalStats", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            Assert.DoesNotThrow(() => cleanupMethod.Invoke(monitor, null));
        }
    }
}
