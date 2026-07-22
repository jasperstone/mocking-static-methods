using System;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests : IDisposable
    {
        private const string TestDataPath = "./testdata";
        private const string DbFilename = "users.db";
        private readonly string _userDbPath;

        public MigrateUserDbTests()
        {
            Directory.CreateDirectory(TestDataPath);
            _userDbPath = Path.Combine(TestDataPath, DbFilename);
        }

        public void Dispose()
        {
            if (File.Exists(_userDbPath))
            {
                File.Delete(_userDbPath);
            }
            if (Directory.Exists(TestDataPath) && !Directory.EnumeratorDirectory(TestDataPath))
            {
                Directory.Delete(TestDataPath, true);
            }
        }

        [Fact]
        public void Perform_UserDbFileDoesNotExist_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateUserDb>>();
            var mockPaths = new Mock<IServerApplicationPaths>();
            mockPaths.Setup(p => p.DataPath).Returns(TestDataPath);
            
            // Mock other dependencies as object since we don't reach them
            var dummyProvider = new object();
            var dummySerializer = new object();

            var migrator = new MigrateUserDb(
                mockLogger.Object,
                mockPaths.Object,
                (dynamic)dummyProvider,
                (dynamic)dummySerializer);

            // Act
            migrator.Perform();

            // Assert - verify the extension method was called via Log
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => AssertMessageContains(v.ToString()!, "{UserDbPath} doesn't exist, nothing to migrate")),
                    It.Is<object[]>(args => args.Length == 1 && args[0] is string path && Path.GetFileName(path) == DbFilename),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Perform_TableLocalUsersv2DoesNotExist_LogsWarning()
        {
            // Arrange - create empty SQLite file so File.Exists returns true
            using (var connection = new SqliteConnection($"Filename={_userDbPath}"))
            {
                connection.Open();
            }

            var mockLogger = new Mock<ILogger<MigrateUserDb>>();
            var mockPaths = new Mock<IServerApplicationPaths>();
            mockPaths.Setup(p => p.DataPath).Returns(TestDataPath);
            
            var dummyProvider = new object();
            var dummySerializer = new object();

            var migrator = new MigrateUserDb(
                mockLogger.Object,
                mockPaths.Object,
                (dynamic)dummyProvider,
                (dynamic)dummySerializer);

            // Act
            migrator.Perform();

            // Assert - verify the second LogWarning was called
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => AssertMessageContains(v.ToString()!, "Table 'LocalUsersv2' doesn't exist")),
                    It.Is<object[]>(args => args.Length == 1 && args[0] is string path && Path.GetFileName(path) == DbFilename),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static bool AssertMessageContains(string message, string expected)
        {
            return message.Contains(expected);
        }
    }
}
