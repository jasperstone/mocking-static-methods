using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using Xunit;
using Jellyfin.Server.Migrations.Routines;

public class MigrateUserDbTests
{
    [Fact]
    public void Perform_LogsWarning_WhenUserDbPathDoesNotExist()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateUserDb>>();
        var pathsMock = new Mock<IServerApplicationPaths>();
        var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
        var xmlSerializerMock = new Mock<IXmlSerializer>();

        pathsMock.Setup(p => p.DataPath).Returns("fakeDataPath");

        var migrateUserDb = new MigrateUserDb(
            loggerMock.Object,
            pathsMock.Object,
            providerMock.Object,
            xmlSerializerMock.Object);

        // Act
        migrateUserDb.Perform();

        // Assert
        loggerMock.Verify(
            l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()),
            Times.Once);
    }
}
