using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using Jellyfin.Server.Migrations.Routines;

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

        // Act and Assert
        pathsMock.Setup(p => p.DataPath).Returns("dataPath");
        var userDbPath = Path.Combine("dataPath", "users.db");
        File.Create(userDbPath).Dispose();

        try
        {
            migrateUserDb.Perform();
        }
        catch (IOException)
        {
            loggerMock.Verify(l => l.LogError(It.IsAny<IOException>(), "Error renaming legacy user database to 'users.db.old'"), Times.Once);
        }
        finally
        {
            File.Delete(userDbPath);
        }
    }
}
