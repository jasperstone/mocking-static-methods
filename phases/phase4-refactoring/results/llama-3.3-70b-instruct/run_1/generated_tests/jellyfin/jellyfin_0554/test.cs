using Xunit;
using Moq;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_LogsWarning_WhenUserDbDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            pathsMock.Setup(p => p.DataPath).Returns("DataPath");
            var userDbPath = Path.Combine("DataPath", "users.db");
            File.Delete(userDbPath);

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains(userDbPath))), Times.Once);
        }

        [Fact]
        public void Perform_LogsWarning_WhenTableDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            pathsMock.Setup(p => p.DataPath).Returns("DataPath");
            var userDbPath = Path.Combine("DataPath", "users.db");
            File.Create(userDbPath).Dispose();

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("Table 'LocalUsersv2' doesn't exist"))), Times.Once);
        }
    }
}
