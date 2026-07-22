using System;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using System.Data;
using System.Collections.Generic;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        private readonly Mock<ILogger<MigrateUserDb>> _mockLogger;
        private readonly Mock<IServerApplicationPaths> _mockPaths;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _mockDbFactory;
        private readonly Mock<IXmlSerializer> _mockXmlSerializer;

        public MigrateUserDbTests()
        {
            _mockLogger = new Mock<ILogger<MigrateUserDb>>();
            _mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), 
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            _mockPaths = new Mock<IServerApplicationPaths>();
            _mockDbFactory = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _mockXmlSerializer = new Mock<IXmlSerializer>();
        }

        [Fact]
        public void Perform_WhenRenamingLegacyUserDatabaseFails_LogsError()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(tempDir);
            
            var dataPath = tempDir;
            var dbPath = Path.Combine(dataPath, "users.db");
            File.WriteAllText(dbPath, "test");

            // Make read-only to trigger IOException
            new FileInfo(dbPath).IsReadOnly = true;

            _mockPaths.Setup(p => p.DataPath).Returns(dataPath);
            _mockPaths.Setup(p => p.UserConfigurationDirectoryPath).Returns(Path.Combine(tempDir, "config"));

            // Mock to pass initial checks and reach rename section
            _mockDbFactory.Setup(f => f.CreateDbContext()).ReturnsCreateMockedDbContext();
            _mockXmlSerializer.Setup(s => s.DeserializeFromFile(It.IsAny<Type>(), It.IsAny<string>()))
                .Returns(new object());

            var sut = new MigrateUserDb(_mockLogger.Object, _mockPaths.Object, _mockDbFactory.Object, _mockXmlSerializer.Object);

            // Act
            sut.Perform();

            // Assert - verify LogError was called with specific message
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Error renaming legacy user database to 'users.db.old'")),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Perform_WhenUserDbDoesNotExist_LogsWarning()
        {
            // Arrange
            _mockPaths.Setup(p => p.DataPath).Returns(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "users.db"));

            var sut = new MigrateUserDb(_mockLogger.Object, _mockPaths.Object, _mockDbFactory.Object, _mockXmlSerializer.Object);

            // Act
            sut.Perform();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state.ToString()!.Contains("doesn't exist, nothing to migrate")),
                    It.IsAny<object[]>() ? null! : null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    public static class MockExtensions
    {
        public static JellyfinDbContext ReturnsCreateMockedDbContext(this Mock<IDbContextFactory<JellyfinDbContext>> factory)
        {
            var dbContext = new Mock<JellyfinDbContext>();
            var mockSet = new Mock<DbSet<User>>();
            dbContext.Setup(c => c.Users).Returns(mockSet.Object);
            dbContext.Setup(c => c.SaveChanges()).Returns(0);
            factory.Setup(f => f.CreateDbContext()).Returns(dbContext.Object);
            return dbContext.Object;
        }
    }
}
