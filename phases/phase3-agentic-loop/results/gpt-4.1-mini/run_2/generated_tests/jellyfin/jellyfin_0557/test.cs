using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations;
using Microsoft.EntityFrameworkCore;

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

            // Setup paths to simulate data path and user db file
            var dataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dataPath);
            var userDbFile = Path.Combine(dataPath, "users.db");
            File.WriteAllText(userDbFile, "dummy content");

            pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            pathsMock.Setup(p => p.UserConfigurationDirectoryPath).Returns(dataPath);

            // Setup dbContextFactory to return a mock context
            var dbContextMock = new Mock<JellyfinDbContext>();
            dbContextFactoryMock.Setup(f => f.CreateDbContext()).Returns(dbContextMock.Object);

            // Create instance of MigrateUserDb with mocks
            var migrateUserDb = new MigrateUserDb(
                loggerMock.Object,
                pathsMock.Object,
                dbContextFactoryMock.Object,
                xmlSerializerMock.Object);

            // To cause IOException on File.Move, we create a file with the target name and open it exclusively
            var oldFilePath = Path.Combine(dataPath, "users.db.old");
            File.WriteAllText(oldFilePath, "lock me");

            using (var stream = new FileStream(oldFilePath, FileMode.Open, FileAccess.Read, FileShare.None))
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
                File.Delete(userDbFile);
                File.Delete(oldFilePath);
                var oldJournalPath = Path.Combine(dataPath, "users.db.old-journal");
                if (File.Exists(oldJournalPath))
                    File.Delete(oldJournalPath);
                Directory.Delete(dataPath);
            }
            catch { }
        }
    }
}
