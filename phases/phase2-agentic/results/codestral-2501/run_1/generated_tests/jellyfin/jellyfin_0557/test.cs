using System;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void LogError_WhenIOExceptionOccurs_ShouldLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            var dataPath = "testDataPath";
            var dbFilename = "users.db";
            var dbPath = Path.Combine(dataPath, dbFilename);

            pathsMock.Setup(p => p.DataPath).Returns(dataPath);

            // Simulate IOException during file move
            var exception = new IOException("Simulated IOException");

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<IOException, string>>()),
                Times.Once);
        }
    }
}
