using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        private readonly Mock<IServerApplicationPaths> _mockPaths;
        private readonly Mock<IXmlSerializer> _mockXmlSerializer;
        private readonly Mock<ILogger<MigrateUserDb>> _mockLogger;

        public MigrateUserDbTests()
        {
            _mockPaths = new Mock<IServerApplicationPaths>();
            _mockXmlSerializer = new Mock<IXmlSerializer>();
            _mockLogger = new Mock<ILogger<MigrateUserDb>>();
            _mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
        }

        [Fact]
        public void Perform_WhenUserDbFileDoesNotExist_LogsWarning()
        {
            // Arrange
            var dataPath = "/test/data";
            var userDbPath = Path.Combine(dataPath, "users.db");
            _mockPaths.Setup(p => p.DataPath).Returns(dataPath);
            
            // Create a fake directory structure where users.db doesn't exist
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var migration = new MigrateUserDb(
                    _mockLogger.Object,
                    _mockPaths.Object,
                    null!, // not used when file doesn't exist
                    _mockXmlSerializer.Object);

                // Act
                migration.Perform();

                // Assert
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(v => v.ToString()!.Contains(userDbPath) && v.ToString()!.Contains("doesn't exist")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }
    }
}
