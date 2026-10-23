using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Server.Implementations;
using Jellyfin.Database.Implementations;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_WhenIOExceptionOccurs_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            // Setup paths to simulate data path and user config directory
            var dataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dataPath);
            pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            pathsMock.Setup(p => p.UserConfigurationDirectoryPath).Returns(dataPath);

            // Create a dummy users.db file to pass the initial check
            var userDbPath = Path.Combine(dataPath, "users.db");
            File.WriteAllText(userDbPath, "dummy content");

            // Setup dbContextFactory to return a mock context
            var dbContextMock = new Mock<JellyfinDbContext>();
            dbContextFactoryMock.Setup(f => f.CreateDbContext()).Returns(dbContextMock.Object);

            // Create instance of MigrateUserDb
            var migrateUserDb = new MigrateUserDb(
                loggerMock.Object,
                pathsMock.Object,
                dbContextFactoryMock.Object,
                xmlSerializerMock.Object);

            // Lock the file to cause IOException on move
            using (var stream = new FileStream(userDbPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // Act
                migrateUserDb.Perform();
            }

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
    }
}
