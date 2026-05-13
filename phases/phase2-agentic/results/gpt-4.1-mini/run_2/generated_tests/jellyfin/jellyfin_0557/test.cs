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
        public void Perform_LogsErrorWhenIOExceptionOccursDuringFileMove()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            // Setup paths to simulate existing user DB file
            var dataPath = Path.GetTempPath();
            pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            pathsMock.Setup(p => p.UserConfigurationDirectoryPath).Returns(dataPath);

            // Create a dummy user DB file to pass the existence check
            var userDbPath = Path.Combine(dataPath, "users.db");
            if (!File.Exists(userDbPath))
            {
                File.WriteAllText(userDbPath, "dummy");
            }

            // Setup dbContextFactory to return a mock DbContext
            var dbContextMock = new Mock<JellyfinDbContext>();
            dbContextFactoryMock.Setup(f => f.CreateDbContext()).Returns(dbContextMock.Object);

            // Create instance of MigrateUserDb with mocks
            var migrateUserDb = new MigrateUserDb(
                loggerMock.Object,
                pathsMock.Object,
                dbContextFactoryMock.Object,
                xmlSerializerMock.Object);

            // To simulate IOException on File.Move, we use a wrapper or shim.
            // Since File.Move is static, we cannot mock it directly.
            // Instead, we simulate by creating a file and locking it to cause IOException.

            // Lock the file to cause IOException on move
            using (var stream = new FileStream(userDbPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // Act
                migrateUserDb.Perform();

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error renaming legacy user database to 'users.db.old'")),
                        It.IsAny<IOException>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            // Cleanup
            try
            {
                File.Delete(userDbPath);
                File.Delete(userDbPath + ".old");
                File.Delete(userDbPath + ".old-journal");
            }
            catch { }
        }
    }
}
