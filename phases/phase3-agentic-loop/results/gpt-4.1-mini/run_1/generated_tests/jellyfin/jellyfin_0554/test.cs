using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_LogsWarningWhenUserDbDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            // Setup DataPath to a temp directory
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            pathsMock.Setup(p => p.DataPath).Returns(tempDir);

            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("doesn't exist, nothing to migrate")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    // Dummy interfaces to allow compilation of the test
    public interface IServerApplicationPaths
    {
        string DataPath { get; }
    }

    public interface IDbContextFactory<T>
    {
        T CreateDbContext();
    }

    public class JellyfinDbContext
    {
    }

    public interface IXmlSerializer
    {
    }
}
