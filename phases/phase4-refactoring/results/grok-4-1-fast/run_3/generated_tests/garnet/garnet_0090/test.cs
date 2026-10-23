using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace Garnet.cluster.Server.Migration.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_TransmitSlotsFailed_CreatesCorrectMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var cursor = 100L;
            var current = 150L;
            var count = 5;

            mockLogger.Setup(l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((func, t) =>
                {
                    try
                    {
                        var state = new { cursor = cursor.ToString(), current = current.ToString(), count = count.ToString() };
                        var message = func(state, null);
                        return message.Contains("TransmitSlots failed for 100 to 150 (with 5 keys)");
                    }
                    catch
                    {
                        return false;
                    }
                })))
                .Verifiable(Times.Once());

            // Act - Directly test the extension method usage pattern
            mockLogger.Object.LogWarning("TransmitSlots failed for {cursor} to {current} (with {count} keys)", cursor, current, count);

            // Assert
            mockLogger.Verify();
        }

        [Fact]
        public void LogWarning_MainStoreScanRange_CreatesCorrectMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var workerStartAddress = 100L;
            var workerEndAddress = 200L;

            mockLogger.Setup(l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((func, t) =>
                {
                    try
                    {
                        var state = new { workerStartAddress = workerStartAddress.ToString(), workerEndAddress = workerEndAddress.ToString() };
                        var message = func(state, null);
                        return message.Contains("<MainStore> migrate keys (namespaces) scan range [100, 200]");
                    }
                    catch
                    {
                        return false;
                    }
                })))
                .Verifiable(Times.Once());

            // Act
            mockLogger.Object.LogWarning("<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]", workerStartAddress, workerEndAddress);

            // Assert
            mockLogger.Verify();
        }

        [Fact]
        public void LogWarning_ScanDiscoveredKeys_CreatesCorrectMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var cursor = 100L;
            var current = 150L;
            var count = 42;

            mockLogger.Setup(l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((func, t) =>
                {
                    try
                    {
                        var state = new { cursor = cursor.ToString(), current = current.ToString(), count = count.ToString() };
                        var message = func(state, null);
                        return message.Contains("Scan from 100 to 150 and discovered 42 keys");
                    }
                    catch
                    {
                        return false;
                    }
                })))
                .Verifiable(Times.Once());

            // Act
            mockLogger.Object.LogWarning("Scan from {cursor} to {current} and discovered {count} keys", cursor, current, count);

            // Assert
            mockLogger.Verify();
        }
    }
}
