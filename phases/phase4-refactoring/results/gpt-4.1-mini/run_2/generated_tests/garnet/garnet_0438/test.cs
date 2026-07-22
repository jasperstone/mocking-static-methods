using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class GarnetServerMonitorTests
    {
        [Fact]
        public void CleanupGlobalStats_LogsResettingCommandStats_WhenCommandStatsFlagIsTrue()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<object>(); // Use object as placeholder for StoreWrapper
            var mockServer = new Mock<object>(); // Use object as placeholder for IGarnetServer
            var servers = Array.CreateInstance(mockServer.Object.GetType(), 1);
            servers.SetValue(mockServer.Object, 0);

            // Use reflection to get the internal GarnetServerMonitor type
            var assembly = typeof(Microsoft.Extensions.Logging.ILogger).Assembly;
            var garnetAssembly = typeof(Microsoft.Extensions.Logging.ILogger).Assembly; // fallback, will replace below

            // Find GarnetServerMonitor type by scanning loaded assemblies
            Type monitorType = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                monitorType = asm.GetType("Garnet.server.GarnetServerMonitor");
                if (monitorType != null) break;
            }
            Assert.NotNull(monitorType);

            // Create dummy options object with required properties
            var optsType = monitorType.Assembly.GetType("Garnet.server.GarnetServerOptions");
            Assert.NotNull(optsType);
            var opts = Activator.CreateInstance(optsType);
            optsType.GetProperty("MetricsSamplingFrequency")?.SetValue(opts, 1);
            optsType.GetProperty("LatencyMonitor")?.SetValue(opts, false);
            optsType.GetProperty("CommandStatsMonitor")?.SetValue(opts, true);

            // Create instance of GarnetServerMonitor via reflection
            var monitor = Activator.CreateInstance(monitorType, mockStoreWrapper.Object, opts, servers, mockLogger.Object);

            // Access resetEventFlags dictionary
            var resetEventFlagsField = monitorType.GetField("resetEventFlags", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(resetEventFlagsField);
            var resetEventFlags = resetEventFlagsField.GetValue(monitor) as IDictionary;
            Assert.NotNull(resetEventFlags);

            // Find InfoMetricsType enum type
            var infoMetricsType = resetEventFlags.GetType().GetGenericArguments()[0];
            if (infoMetricsType == null)
            {
                // fallback: find enum type by name in assembly
                infoMetricsType = monitorType.Assembly.GetType("Garnet.server.InfoMetricsType");
            }
            Assert.NotNull(infoMetricsType);

            // Get enum value COMMANDSTATS
            var commandStatsValue = Enum.Parse(infoMetricsType, "COMMANDSTATS");

            // Set the COMMANDSTATS flag to true
            resetEventFlags[commandStatsValue] = true;

            // Get private CleanupGlobalStats method
            var cleanupMethod = monitorType.GetMethod("CleanupGlobalStats", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(cleanupMethod);

            // Act
            cleanupMethod.Invoke(monitor, null);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Resetting command stats")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
