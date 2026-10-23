using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_LogsExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Database Purged";

            // Act
            loggerMock.Object.LogInformation(message);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
