using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;

namespace Jellyfin.Tests.Migrations
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void LogError_ShouldBeCalled_WhenIOExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var exception = new IOException("Simulated IO error");

            // Act
            loggerMock.Object.LogError(exception, "Error renaming legacy user database to 'users.db.old'");

            // Assert
            loggerMock.Verify(
                x => x.LogError(exception, "Error renaming legacy user database to 'users.db.old'"),
                Times.Once);
        }
    }
}
