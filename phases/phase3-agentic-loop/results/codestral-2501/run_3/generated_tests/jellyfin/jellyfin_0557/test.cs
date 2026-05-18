using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;
using System.IO;
using MediaBrowser.Controller;
using Jellyfin.Data;
using Microsoft.EntityFrameworkCore;
using MediaBrowser.Model.Serialization;
using Jellyfin.Database.Implementations;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_ShouldLogError_WhenRenamingLegacyUserDatabaseFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            pathsMock.Setup(p => p.DataPath).Returns("dataPath");
            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<IOException>(),
                    "Error renaming legacy user database to 'users.db.old'",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
