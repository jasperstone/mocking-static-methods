using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;
using Tsavorite.core;
using Garnet.client;
using Garnet.server;

namespace Garnet.cluster.Server.Migration.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_TransmitSlotsFailed_CalledWithCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateOperation>>();
            long cursor = 100L;
            long current = 200L;
            int count = 3;

            // Act
            mockLogger.Object.LogWarning("TransmitSlots failed for {cursor} to {current} (with {count} keys)", cursor, current, count);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TransmitSlots failed for 100 to 200 (with 3 keys")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_ScanRange_CalledWithCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateOperation>>();
            long workerStartAddress = 100L;
            long workerEndAddress = 200L;

            // Act
            mockLogger.Object.LogWarning("<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]", workerStartAddress, workerEndAddress);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("scan range [100, 200]")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_ScanDiscoveredKeys_CalledWithCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateOperation>>();
            long cursor = 100L;
            long current = 150L;
            int count = 5;

            // Act
            mockLogger.Object.LogWarning("Scan from {cursor} to {current} and discovered {count} keys", cursor, current, count);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Scan from 100 to 150 and discovered 5 keys")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
