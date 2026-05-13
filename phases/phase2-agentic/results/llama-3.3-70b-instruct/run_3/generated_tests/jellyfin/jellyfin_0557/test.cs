using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using Jellyfin.Server.Migrations.Routines;

public class MigrateUserDbTests
{
    [Fact]
    public void LogError_Called_WhenIOExceptionOccurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateUserDb>>();
        var pathsMock = new Mock<IServerApplicationPaths>();
        var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var xmlSerializerMock = new Mock<IXmlSerializer>();
        var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

        // Act and Assert
        var dataPath = "path";
        pathsMock.Setup(p => p.DataPath).Returns(dataPath);
        var userDbPath = Path.Combine(dataPath, "users.db");
        File.Create(userDbPath).Dispose();
        var journalPath = Path.Combine(dataPath, "users.db-journal");
        File.Create(journalPath).Dispose();

        migrateUserDb.Perform();

        loggerMock.Verify(l => l.LogError(It.IsAny<IOException>(), It.IsAny<string>()), Times.Once);
    }
}
