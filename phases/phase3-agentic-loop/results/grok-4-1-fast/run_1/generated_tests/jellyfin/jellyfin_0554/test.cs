using System;
using System.IO;
using MediaBrowser.Controller;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Jellyfin.Server.Migrations.Routines;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_WhenUserDbDoesNotExist_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            loggerMock.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("{UserDbPath} doesn't exist, nothing to migrate")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            var pathsMock = new Mock<IServerApplicationPaths>();
            pathsMock.Setup(p => p.DataPath).Returns("/data");
            
            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, null!, null!);

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("{UserDbPath} doesn't exist, nothing to migrate")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
