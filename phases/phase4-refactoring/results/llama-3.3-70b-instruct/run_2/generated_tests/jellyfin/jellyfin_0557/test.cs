using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Server.Interfaces;

public class MigrateUserDbTests
{
    [Fact]
    public void Perform_LogsErrorWhenFileMoveFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateUserDb>>();
        var pathsMock = new Mock<IServerApplicationPaths>();
        var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var xmlSerializerMock = new Mock<IXmlSerializer>();

        var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

        // Act and Assert
        var dataPath = "dataPath";
        pathsMock.Setup(p => p.DataPath).Returns(dataPath);
        var userDbPath = Path.Combine(dataPath, "users.db");
        var oldUserDbPath = Path.Combine(dataPath, "users.db.old");

        // Make the file move fail
        using (var fileStream = File.Create(oldUserDbPath))
        {
            // Try to move the file
            try
            {
                File.Move(userDbPath, oldUserDbPath);
            }
            catch (IOException e)
            {
                // Verify the error is logged
                loggerMock.Verify(l => l.LogError(e, "Error renaming legacy user database to 'users.db.old'"), Times.Once);
            }
            finally
            {
                // Clean up
                File.Delete(oldUserDbPath);
            }
        }
    }
}
