using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;

namespace Garnet.server.Tests
{
    public class GarnetServerMonitorTests
    {
        [Fact]
        public void CleanupGlobalStats_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var garnetServerMonitor = new GarnetServerMonitor(null, null, null, loggerMock.Object);
            var type = typeof(GarnetServerMonitor);
            var fieldInfo = type.GetField("resetEventFlags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var resetEventFlags = (Dictionary<InfoMetricsType, bool>)fieldInfo.GetValue(garnetServerMonitor);
            resetEventFlags[InfoMetricsType.STATS] = true;

            // Act
            var methodInfo = type.GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Invoke(garnetServerMonitor, null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Resetting latency metrics for commands"), Times.Once);
        }

        [Fact]
        public void CleanupGlobalStats_LogInformationNotCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var garnetServerMonitor = new GarnetServerMonitor(null, null, null, loggerMock.Object);
            var type = typeof(GarnetServerMonitor);
            var fieldInfo = type.GetField("resetEventFlags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var resetEventFlags = (Dictionary<InfoMetricsType, bool>)fieldInfo.GetValue(garnetServerMonitor);
            resetEventFlags[InfoMetricsType.STATS] = false;

            // Act
            var methodInfo = type.GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Invoke(garnetServerMonitor, null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Resetting latency metrics for commands"), Times.Never);
        }

        [Fact]
        public void CleanupGlobalCommandStats_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var garnetServerMonitor = new GarnetServerMonitor(null, null, null, loggerMock.Object);
            var type = typeof(GarnetServerMonitor);
            var fieldInfo = type.GetField("resetEventFlags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var resetEventFlags = (Dictionary<InfoMetricsType, bool>)fieldInfo.GetValue(garnetServerMonitor);
            resetEventFlags[InfoMetricsType.COMMANDSTATS] = true;

            // Act
            var methodInfo = type.GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Invoke(garnetServerMonitor, null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Resetting command stats"), Times.Once);
        }

        [Fact]
        public void CleanupGlobalCommandStats_LogInformationNotCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var garnetServerMonitor = new GarnetServerMonitor(null, null, null, loggerMock.Object);
            var type = typeof(GarnetServerMonitor);
            var fieldInfo = type.GetField("resetEventFlags", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var resetEventFlags = (Dictionary<InfoMetricsType, bool>)fieldInfo.GetValue(garnetServerMonitor);
            resetEventFlags[InfoMetricsType.COMMANDSTATS] = false;

            // Act
            var methodInfo = type.GetMethod("CleanupGlobalStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Invoke(garnetServerMonitor, null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Resetting command stats"), Times.Never);
        }
    }

    public enum InfoMetricsType : byte
    {
        STATS,
        COMMANDSTATS
    }
}
