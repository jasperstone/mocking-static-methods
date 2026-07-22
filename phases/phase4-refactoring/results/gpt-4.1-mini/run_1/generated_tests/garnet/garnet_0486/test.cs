using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_LogsWarningMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var warningMessage = "Cluster username is not provided, will use new password with existing username";

            // Act
            loggerMock.Object.LogWarning(warningMessage);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(warningMessage)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
