using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using Jellyfin.Server.Migrations.Routines;

public class MigrateUserDbTests
{
    [Fact]
    public void LogError_Called_When_Renaming_Legacy_User_Database_Fails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateUserDb>>();
        var pathsMock = new Mock<IServerApplicationPaths>();
        var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var xmlSerializerMock = new Mock<IXmlSerializer>();
        var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

        // Act
        try
        {
            File.Move("users.db", "users.db.old");
        }
        catch (IOException e)
        {
            loggerMock.Object.LogError(e, "Error renaming legacy user database to 'users.db.old'");
        }

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<IOException>(), "Error renaming legacy user database to 'users.db.old'"), Times.Once);
    }
}
