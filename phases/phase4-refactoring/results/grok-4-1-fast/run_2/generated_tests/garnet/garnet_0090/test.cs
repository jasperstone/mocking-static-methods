using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Garnet.cluster;
using Tsavorite.core;

namespace Garnet.cluster.Server.Migration.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_TransmitSlotsFailed_CalledWithCorrectFormatAndArguments()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;
            
            var cursor = 100L;
            var current = 150L;
            var count = 5;

            // Act
            logger.LogWarning("TransmitSlots failed for {cursor} to {current} (with {count} keys)", cursor, current, count);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TransmitSlots failed for 100 to 150 (with 5 keys)")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_MainStoreScanRange_CalledWithCorrectFormatAndArguments()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;
            
            var workerStartAddress = 100L;
            var workerEndAddress = 200L;

            // Act
            logger.LogWarning("<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]", workerStartAddress, workerEndAddress);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("scan range [100, 200]")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_ScanDiscoveredKeys_CalledWithCorrectFormatAndArguments()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;
            
            var cursor = 100L;
            var current = 150L;
            var count = 42;

            // Act
            logger.LogWarning("Scan from {cursor} to {current} and discovered {count} keys", cursor, current, count);

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Scan from 100 to 150 and discovered 42 keys")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
