using System;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        private const string TestDataPath = "/test/data";
        private const string DbFilename = "users.db";
        private const string UserDbPath = TestDataPath + "/" + DbFilename;

        [Fact]
        public void Perform_UserDbDoesNotExist_LogsWarningAndReturns()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateUserDb>>();
            var mockPaths = new Mock<IServerApplicationPaths>();
            mockPaths.Setup(p => p.DataPath).Returns(TestDataPath);

            // Create dummy implementations for dependencies that don't affect the early return path
            var dummyProvider = new Mock<object>().Object;
            var dummyXmlSerializer = new Mock<object>().Object;
            var migration = new MigrateUserDb(
                mockLogger.Object,
                mockPaths.Object,
                new Mock<IDbContextFactory<object>>().Object,
                new Mock<IXmlSerializer>().Object);

            // Act
            migration.Perform();

            // Assert - Verify the specific LogWarning extension call on line 63
            mockLogger.Verify(
                x => x.LogWarning(
                    "{UserDbPath} doesn't exist, nothing to migrate",
                    UserDbPath),
                Times.Once);
        }
    }
}
