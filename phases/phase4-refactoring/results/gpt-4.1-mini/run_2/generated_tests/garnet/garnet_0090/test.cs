using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_CalledWithExpectedMessageAndParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();

            var workerStartAddress = 10L;
            var workerEndAddress = 20L;

            // Act
            mockLogger.Object.LogWarning("<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]", workerStartAddress, workerEndAddress);

            // Assert
            mockLogger.Verify(l => l.LogWarning(
                "<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]",
                workerStartAddress,
                workerEndAddress), Times.Once);
        }
    }
}
