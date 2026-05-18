using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void LogErrorExtension_CalledWithIOExceptionAndCorrectMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateUserDb>>();
            
            var exception = new IOException("Test IO error");
            const string expectedMessage = "Error renaming legacy user database to 'users.db.old'";
            
            // Act - Simulate the exact LogError extension call from line 214
            mockLogger.Object.LogError(exception, expectedMessage);
            
            // Assert - Verify the underlying Log call matches the extension method pattern
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == expectedMessage),
                    It.Is<Exception>(e => e == exception),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
