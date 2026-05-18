using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_LogsErrorWhenRenamingLegacyUserDatabaseFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            var dataPath = "dataPath";
            pathsMock.Setup(p => p.DataPath).Returns(dataPath);

            var dbFilename = "users.db";
            var userDbPath = Path.Combine(dataPath, dbFilename);

            // Act and Assert
            loggerMock.Setup(l => l.LogError(It.IsAny<Exception>(), "Error renaming legacy user database to 'users.db.old'")).Verifiable();
            try
            {
                File.Move(userDbPath, Path.Combine(dataPath, dbFilename + ".old"));
            }
            catch (IOException e)
            {
                loggerMock.Object.LogError(e, "Error renaming legacy user database to 'users.db.old'");
            }

            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error renaming legacy user database to 'users.db.old'"), Times.Once);
        }
    }
}
