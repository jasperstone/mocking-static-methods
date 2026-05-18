using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void LogError_CalledWithIOException_LogsErrorWithCorrectTemplate()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var mockLogger = loggerMock.Object;
            var ioException = new IOException("Test IO exception");

            // Act
            mockLogger.LogError(ioException, "Error renaming legacy user database to 'users.db.old'");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error renaming legacy user database to 'users.db.old'")),
                    ioException,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
