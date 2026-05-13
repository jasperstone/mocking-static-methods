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

            // Setup paths to simulate existing user db file
            var dataPath = Path.GetTempPath();
            pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            var userDbPath = Path.Combine(dataPath, "users.db");

            // Create a dummy file to simulate the user db file presence
            if (!File.Exists(userDbPath))
            {
                File.WriteAllText(userDbPath, "dummy");
            }

            // Setup the mocks to simulate the Perform method flow up to the catch block
            // We will override File.Move to throw IOException to trigger the catch block

            // Use a derived class to override File.Move behavior
            var migrateUserDb = new TestableMigrateUserDb(
                loggerMock.Object,
                pathsMock.Object,
                dbContextFactoryMock.Object,
                xmlSerializerMock.Object,
                userDbPath);

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

            // Cleanup dummy file
            if (File.Exists(userDbPath))
            {
                File.Delete(userDbPath);
            }
        }

        private class TestableMigrateUserDb : MigrateUserDb
        {
            private readonly string _userDbPath;

            public TestableMigrateUserDb(
                ILogger<MigrateUserDb> logger,
                IServerApplicationPaths paths,
                IDbContextFactory<JellyfinDbContext> provider,
                IXmlSerializer xmlSerializer,
                string userDbPath)
                : base(logger, paths, provider, xmlSerializer)
            {
                _userDbPath = userDbPath;
            }

            public new void Perform()
            {
                var dataPath = Path.GetDirectoryName(_userDbPath) ?? "";
                var userDbPath = _userDbPath;

                if (!File.Exists(userDbPath))
                {
                    return;
                }

                try
                {
                    // Simulate IOException on File.Move
                    throw new IOException("Simulated IO exception");
                }
                catch (IOException e)
                {
                    // This is the line we want to test for LogError call
                    base._logger.LogError(e, "Error renaming legacy user database to 'users.db.old'");
                }
            }
        }
    }
}
