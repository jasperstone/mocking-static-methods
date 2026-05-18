using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Garnet.client;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Tsavorite.core;
using Xunit;

namespace Garnet.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_TransmitSlotsFailed_CalledWithCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var cursor = 100L;
            var current = 150L;
            var count = 5;

            // Act
            mockLogger.Object.LogWarning("TransmitSlots failed for {cursor} to {current} (with {count} keys)", cursor, current, count);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TransmitSlots failed") && v.ToString().Contains(cursor.ToString()) && v.ToString().Contains(current.ToString()) && v.ToString().Contains(count.ToString())),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
                ),
                Times.Once);
        }

        [Fact]
        public void LogWarning_MainStoreScanRange_CalledWithCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var workerStartAddress = 100L;
            var workerEndAddress = 200L;

            // Act
            mockLogger.Object.LogWarning("<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]", workerStartAddress, workerEndAddress);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("<MainStore>") && v.ToString().Contains(workerStartAddress.ToString()) && v.ToString().Contains(workerEndAddress.ToString())),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once);
        }

        [Fact]
        public void LogWarning_ScanDiscoveredKeys_CalledWithCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var cursor = 100L;
            var current = 150L;
            var count = 10;

            // Act
            mockLogger.Object.LogWarning("Scan from {cursor} to {current} and discovered {count} keys", cursor, current, count);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Scan from") && v.ToString().Contains(cursor.ToString()) && v.ToString().Contains(current.ToString()) && v.ToString().Contains(count.ToString())),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once);
        }
    }
}
