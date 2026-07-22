using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Server.Implementations.Users;
using Jellyfin.Database.Implementations;
using MediaBrowser.Model.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        private class TestServerApplicationPaths : IServerApplicationPaths
        {
            public string DataPath { get; set; }
            public string UserConfigurationDirectoryPath { get; set; }
        }

        private class TestDbContextFactory : IDbContextFactory<JellyfinDbContext>
        {
            public JellyfinDbContext CreateDbContext()
            {
                // Return a dummy DbContext with empty Users DbSet
                var options = new DbContextOptionsBuilder<JellyfinDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;
                return new JellyfinDbContext(options);
            }
        }

        private class DummyXmlSerializer : IXmlSerializer
        {
            public object DeserializeFromFile(Type type, string filePath)
            {
                if (type == typeof(UserConfiguration))
                    return new UserConfiguration();
                if (type == typeof(UserPolicy))
                    return new UserPolicy();
                return null!;
            }
        }

        [Fact]
        public void Perform_LogsErrorWhenIOExceptionOccursDuringFileMove()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var userDbPath = Path.Combine(tempDir, "users.db");
            File.WriteAllText(userDbPath, "dummy content");

            var paths = new TestServerApplicationPaths
            {
                DataPath = tempDir,
                UserConfigurationDirectoryPath = tempDir
            };

            var dbContextFactory = new TestDbContextFactory();
            var xmlSerializer = new DummyXmlSerializer();

            var migrateUserDb = new MigrateUserDb(
                loggerMock.Object,
                paths,
                dbContextFactory,
                xmlSerializer);

            // Lock the file to cause IOException on File.Move
            using (var stream = File.Open(userDbPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                // Act
                migrateUserDb.Perform();
            }

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
                File.Delete(Path.Combine(tempDir, "users.db.old"));
                File.Delete(Path.Combine(tempDir, "users.db.old-journal"));
                Directory.Delete(tempDir);
            }
            catch { }
        }
    }
}
