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

            // Setup paths to simulate existing user database file
            var dataPath = Path.GetTempPath();
            pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            pathsMock.Setup(p => p.UserConfigurationDirectoryPath).Returns(dataPath);

            // Create a dummy file to simulate the user database file presence
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

            // To simulate IOException on File.Move, we will override File.Move using a shim or wrapper.
            // Since we cannot override static methods easily, we simulate by creating a directory with the same name as the target file,
            // so File.Move will throw IOException.

            var oldFilePath = Path.Combine(dataPath, "users.db.old");
            var oldJournalPath = Path.Combine(dataPath, "users.db.old-journal");
            var journalPath = Path.Combine(dataPath, "users.db-journal");

            try
            {
                // Ensure no file or directory exists at the target rename locations
                if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
                if (Directory.Exists(oldFilePath)) Directory.Delete(oldFilePath);

                if (File.Exists(oldJournalPath)) File.Delete(oldJournalPath);
                if (Directory.Exists(oldJournalPath)) Directory.Delete(oldJournalPath);

                if (File.Exists(journalPath)) File.Delete(journalPath);
                if (Directory.Exists(journalPath)) Directory.Delete(journalPath);

                // Create a directory where the file rename target is expected to be, to cause IOException on File.Move
                Directory.CreateDirectory(oldFilePath);

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
            finally
            {
                // Cleanup
                if (Directory.Exists(oldFilePath)) Directory.Delete(oldFilePath);
                if (File.Exists(userDbPath)) File.Delete(userDbPath);
            }
        }
    }
}
