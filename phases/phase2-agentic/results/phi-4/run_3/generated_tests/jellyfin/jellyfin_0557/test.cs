using System;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class MigrateUserDbTests
{
    [Fact]
    public void Perform_LogsError_WhenIOExceptionOccurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateUserDb>>();
        var pathsMock = new Mock<IServerApplicationPaths>();
        var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var xmlSerializerMock = new Mock<IXmlSerializer>();

        pathsMock.Setup(p => p.DataPath).Returns("test_data_path");

        var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

        // Simulate IOException
        var dataPath = pathsMock.Object.DataPath;
        var dbFilename = "users.db";
        var userDbPath = Path.Combine(dataPath, dbFilename);
        File.WriteAllText(userDbPath, ""); // Create an empty file to simulate existence

        // Act
        migrateUserDb.Perform();

        // Assert
        loggerMock.Verify(
            l => l.LogError(It.IsAny<IOException>(), It.Is<string>(s => s.Contains("Error renaming legacy user database to 'users.db.old'"))),
            Times.Once);
    }
}
