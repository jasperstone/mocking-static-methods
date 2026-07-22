using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_CalledWithExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var message = "Starting cleanup of items from deleted libraries...";

            // Act
            loggerMock.Object.LogInformation(message);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
