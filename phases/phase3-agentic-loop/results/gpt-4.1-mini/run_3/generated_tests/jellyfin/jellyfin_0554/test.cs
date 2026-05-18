using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using Microsoft.EntityFrameworkCore;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_LogsWarning_WhenUserDbDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<Jellyfin.Database.Implementations.JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            var tempDir = Path.GetTempPath();
            pathsMock.Setup(p => p.DataPath).Returns(tempDir);

            var userDbPath = Path.Combine(tempDir, "users.db");
            if (File.Exists(userDbPath))
            {
                File.Delete(userDbPath);
            }

            var migrateUserDb = new MigrateUserDb(
                loggerMock.Object,
                pathsMock.Object,
                providerMock.Object,
                xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("doesn't exist, nothing to migrate")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
