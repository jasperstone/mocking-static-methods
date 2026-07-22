using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_LogsExpectedMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();

            long syncFromAofAddress = 50;
            long beginAofAddress = 100;

            // Act
            mockLogger.Object.LogError(
                "syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}",
                syncFromAofAddress,
                beginAofAddress);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("syncFromAofAddress")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
