using System;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        private readonly Mock<ILogger<MigrateUserDb>> _loggerMock;
        private readonly Mock<object> _pathsMock;
        private readonly Mock<object> _dbContextFactoryMock;
        private readonly Mock<object> _xmlSerializerMock;

        public MigrateUserDbTests()
        {
            _loggerMock = new Mock<ILogger<MigrateUserDb>>();
            _pathsMock = new Mock<object>();
            _dbContextFactoryMock = new Mock<object>();
            _xmlSerializerMock = new Mock<object>();
        }

        [Fact]
        public void Perform_WhenRenamingUserDatabase_ThrowsIOException_LogsError()
        {
            // Arrange
            var dataPath = Path.Combine(Path.GetTempPath(), "jellyfin-test-error-" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(dataPath);
            
            try
            {
                var dbFilename = "users.db";
                var userDbPath = Path.Combine(dataPath, dbFilename);
                File.WriteAllText(userDbPath, "test content"); // Create DB file so migration proceeds
                
                // Setup mocks to bypass early returns and reach file operations
                var pathsMock = new Mock<IServerApplicationPaths>();
                pathsMock.Setup(p => p.DataPath).Returns(dataPath);
                pathsMock.Setup(p => p.UserConfigurationDirectoryPath).Returns(dataPath);
                
                var xmlSerializerMock = new Mock<IXmlSerializer>();
                xmlSerializerMock.Setup(x => x.DeserializeFromFile(It.IsAny<Type>(), It.IsAny<string>()))
                    .Returns(new object());
                
                var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
                var dbContextMock = new Mock<DbContext>();
                dbContextFactoryMock.Setup(f => f.CreateDbContext()).Returns(dbContextMock.Object);
                
                var migrateUserDb = new MigrateUserDb(
                    _loggerMock.Object,
                    pathsMock.Object,
                    dbContextFactoryMock.Object,
                    xmlSerializerMock.Object);

                // Force IOException by making target file read-only
                var targetPath = Path.Combine(dataPath, dbFilename + ".old");
                File.WriteAllText(targetPath, "locked");
                File.SetAttributes(targetPath, FileAttributes.ReadOnly);

                // Act
                migrateUserDb.Perform();

                // Assert - Verify LogError was called with expected message (line 214)
                _loggerMock.Verify(
                    logger => logger.Log(
                        LogLevel.Error,
                        0,
                        It.Is<It.IsAnyFormat<string>>(msg => msg.ToString().Contains("Error renaming legacy user database to 'users.db.old'")),
                        It.IsAny<IOException>(),
                        It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                    Times.Once);
            }
            finally
            {
                try { Directory.Delete(dataPath, true); } catch { }
            }
        }

        [Fact]
        public void Perform_WhenUserDbDoesNotExist_LogsWarning()
        {
            // Arrange
            var pathsMock = new Mock<IServerApplicationPaths>();
            pathsMock.Setup(p => p.DataPath).Returns(Path.GetTempPath());

            var migrateUserDb = new MigrateUserDb(
                _loggerMock.Object,
                pathsMock.Object,
                _dbContextFactoryMock.Object,
                _xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    0,
                    It.Is<It.IsAnyFormat<string>>(msg => msg.ToString().Contains("doesn't exist, nothing to migrate")),
                    It.IsAny<object>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, object?, string>>()),
                Times.Once);
        }
    }
}
