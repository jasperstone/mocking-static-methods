using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_InvokedWithExpectedMessages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            long workerStartAddress = 10;
            long workerEndAddress = 20;
            long cursor = workerStartAddress;
            long current = 15;
            int count = 5;

            // Act
            loggerMock.Object.LogWarning("<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]", workerStartAddress, workerEndAddress);
            loggerMock.Object.LogWarning("Scan from {cursor} to {current} and discovered {count} keys", cursor, current, count);
            loggerMock.Object.LogWarning("TransmitSlots failed for {cursor} to {current} (with {count} keys)", cursor, current, count);

            // Assert
            loggerMock.Verify(l => l.LogWarning("<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]", workerStartAddress, workerEndAddress), Times.Once);
            loggerMock.Verify(l => l.LogWarning("Scan from {cursor} to {current} and discovered {count} keys", cursor, current, count), Times.Once);
            loggerMock.Verify(l => l.LogWarning("TransmitSlots failed for {cursor} to {current} (with {count} keys)", cursor, current, count), Times.Once);
        }
    }
}
