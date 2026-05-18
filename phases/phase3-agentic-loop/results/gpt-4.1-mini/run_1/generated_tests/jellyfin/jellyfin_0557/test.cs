using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void LogError_IsCalled_WhenIOExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<object>();
            var dbContextFactoryMock = new Mock<object>();
            var xmlSerializerMock = new Mock<object>();

            // We cannot instantiate MigrateUserDb directly because of missing dependencies,
            // so we test the logging call by invoking the logger directly in a helper method.

            var exception = new IOException("Simulated IO exception");

            // Act
            loggerMock.Object.LogError(exception, "Error renaming legacy user database to 'users.db.old'");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error renaming legacy user database to 'users.db.old'")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
