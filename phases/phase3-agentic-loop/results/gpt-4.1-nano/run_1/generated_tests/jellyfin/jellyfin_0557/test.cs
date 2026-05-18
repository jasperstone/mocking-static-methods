using System;
using System.IO;
using System.Linq;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Users;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Serialization;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace Jellyfin.Tests.Migrations
{
    public class MigrateUserDbTests
    {
        private readonly Mock<ILogger<MigrateUserDb>> _loggerMock;
        private readonly Mock<IServerApplicationPaths> _pathsMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbContextFactoryMock;
        private readonly Mock<IXmlSerializer> _xmlSerializerMock;
        private readonly string _tempDataPath;

        public MigrateUserDbTests()
        {
            _loggerMock = new Mock<ILogger<MigrateUserDb>>();
            _pathsMock = new Mock<IServerApplicationPaths>();
            _dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _xmlSerializerMock = new Mock<IXmlSerializer>();
            _tempDataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDataPath);
            _pathsMock.Setup(p => p.DataPath).Returns(_tempDataPath);
        }

        [Fact]
        public void Perform_Should_Log_Warning_And_Return_If_UserDb_Does_Not_Exist()
        {
            // Arrange
            var routine = new MigrateUserDb(_loggerMock.Object, _pathsMock.Object, _dbContextFactoryMock.Object, _xmlSerializerMock.Object);
            var userDbPath = Path.Combine(_tempDataPath, "users.db");
            // Ensure file does not exist
            if (File.Exists(userDbPath))
                File.Delete(userDbPath);

            // Act
            routine.Perform();

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("doesn't exist")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Perform_Should_Log_Warning_If_Table_Does_Not_Exist()
        {
            // Arrange
            var userDbPath = Path.Combine(_tempDataPath, "users.db");
            File.WriteAllBytes(userDbPath, new byte[] { 1, 2, 3 }); // dummy file
            var routine = new MigrateUserDb(_loggerMock.Object, _pathsMock.Object, _dbContextFactoryMock.Object, _xmlSerializerMock.Object);

            // Setup SQLite connection to return no tables
            var connectionMock = new Mock<SqliteConnection>($"Filename={userDbPath}");
            connectionMock.Setup(c => c.Query("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='LocalUsersv2';"))
                .Returns(Enumerable.Empty<dynamic>());

            // Act
            routine.Perform();

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("doesn't exist")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Perform_Should_Log_Warning_If_LocalUsersv2_Table_Is_Missing()
        {
            // Arrange
            var userDbPath = Path.Combine(_tempDataPath, "users.db");
            File.WriteAllBytes(userDbPath, new byte[] { 1, 2, 3 });
            var routine = new MigrateUserDb(_loggerMock.Object, _pathsMock.Object, _dbContextFactoryMock.Object, _xmlSerializerMock.Object);

            // Mock the connection to simulate table missing
            var connectionMock = new Mock<SqliteConnection>($"Filename={userDbPath}");
            connectionMock.Setup(c => c.Query("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='LocalUsersv2';"))
                .Returns(new[] new { new { GetInt32 = new Func<int>(() => 0) } });

            // Act
            routine.Perform();

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("doesn't exist")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Perform_Should_Deserialize_And_Add_Users()
        {
            // Arrange
            var userDbPath = Path.Combine(_tempDataPath, "users.db");
            File.WriteAllBytes(userDbPath, new byte[] { 1, 2, 3 });
            var routine = new MigrateUserDb(_loggerMock.Object, _pathsMock.Object, _dbContextFactoryMock.Object, _xmlSerializerMock.Object);

            // Setup mock for database connection and query
            // For brevity, this test will be a high-level outline, as mocking the entire SQLite interaction is complex
            // In real tests, consider abstracting database access for easier mocking

            // Mock deserialization to produce a user mockup
            var mockUser = new UserMockup
            {
                Password = "pass",
                LastLoginDate = DateTime.UtcNow,
                LastActivityDate = DateTime.UtcNow,
                Name = "TestUser",
                ImageInfos = new ItemImageInfo[0]
            };
            _xmlSerializerMock.Setup(s => s.DeserializeFromFile(It.IsAny<Type>(), It.IsAny<string>()))
                .Returns(mockUser);

            // Mock the database context
            var options = new DbContextOptionsBuilder<JellyfinDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var dbContext = new JellyfinDbContext(options);
            _dbContextFactoryMock.Setup(f => f.CreateDbContext()).Returns(dbContext);

            // Act
            routine.Perform();

            // Assert
            Assert.Single(dbContext.Users);
            var user = dbContext.Users.First();
            Assert.Equal("TestUser", user.Name);
            Assert.Equal("pass", user.Password);
        }
    }
}
