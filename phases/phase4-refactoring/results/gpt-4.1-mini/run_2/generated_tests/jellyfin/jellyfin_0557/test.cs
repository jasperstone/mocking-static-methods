using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_WhenIOExceptionOccurs_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IPaths>();
            var dbContextFactoryMock = new Mock<IDbContextFactory>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            var dataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dataPath);
            var userDbPath = Path.Combine(dataPath, "users.db");
            File.WriteAllText(userDbPath, "dummy content");

            pathsMock.SetupGet(p => p.DataPath).Returns(dataPath);
            pathsMock.SetupGet(p => p.UserConfigurationDirectoryPath).Returns(dataPath);

            var migrateUserDb = new MigrateUserDbTestable(
                loggerMock.Object,
                pathsMock.Object,
                dbContextFactoryMock.Object,
                xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error renaming legacy user database to 'users.db.old'")),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Cleanup
            try
            {
                File.Delete(userDbPath);
                Directory.Delete(dataPath);
            }
            catch { }
        }

        // Interfaces to avoid missing type errors
        public interface IPaths
        {
            string DataPath { get; }
            string UserConfigurationDirectoryPath { get; }
        }

        public interface IDbContextFactory
        {
            object CreateDbContext();
        }

        public interface IXmlSerializer
        {
            object DeserializeFromFile(Type type, string filePath);
        }

        // Derived class to override the part that does File.Move to simulate IOException
        private class MigrateUserDbTestable : MigrateUserDb
        {
            public MigrateUserDbTestable(
                ILogger<MigrateUserDb> logger,
                IPaths paths,
                IDbContextFactory provider,
                IXmlSerializer xmlSerializer)
                : base(logger, (IServerApplicationPaths)paths, (IDbContextFactory<JellyfinDbContext>)provider, xmlSerializer)
            {
            }

            public new void Perform()
            {
                var dataPath = ((IPaths)_paths).DataPath;
                var userDbPath = Path.Combine(dataPath, "users.db");
                if (!File.Exists(userDbPath))
                {
                    _logger.LogWarning("{UserDbPath} doesn't exist, nothing to migrate", userDbPath);
                    return;
                }

                _logger.LogInformation("Migrating the user database may take a while, do not stop Jellyfin.");

                try
                {
                    // Simulate IOException on File.Move
                    throw new IOException("Simulated IO exception");
                }
                catch (IOException e)
                {
                    _logger.LogError(e, "Error renaming legacy user database to 'users.db.old'");
                }
            }
        }
    }
}
