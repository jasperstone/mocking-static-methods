using System;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        private readonly Mock<ILogger<MigrateUserDb>> _mockLogger;
        private readonly Mock<IServerApplicationPaths> _mockPaths;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _mockProvider;
        private readonly Mock<IXmlSerializer> _mockXmlSerializer;

        public MigrateUserDbTests()
        {
            _mockLogger = new Mock<ILogger<MigrateUserDb>>();
            _mockPaths = new Mock<IServerApplicationPaths>();
            _mockProvider = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _mockXmlSerializer = new Mock<IXmlSerializer>();
        }

        [Fact]
        public void Perform_WhenRenamingLegacyDatabaseFailsWithIOException_LogsError()
        {
            // Arrange
            var dataPath = Path.Combine(Path.GetTempPath(), "jellyfin_test_" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(dataPath);
            
            _mockPaths.Setup(p => p.DataPath).Returns(dataPath);
            _mockPaths.Setup(p => p.UserConfigurationDirectoryPath).Returns(Path.Combine(dataPath, "users"));

            var userDbPath = Path.Combine(dataPath, "users.db");
            File.WriteAllText(userDbPath, "test");

            // Make file read-only to force IOException
            File.SetAttributes(userDbPath, FileAttributes.ReadOnly);

            // Mock dependencies to avoid database operations
            _mockProvider.Setup(p => p.CreateDbContext()).Throws(new InvalidOperationException("Mocked"));

            var subject = new MigrateUserDb(
                _mockLogger.Object,
                _mockPaths.Object,
                _mockProvider.Object,
                _mockXmlSerializer.Object);

            // Act
            subject.Perform();

            // Cleanup
            try
            {
                File.SetAttributes(userDbPath, FileAttributes.Normal);
                if (File.Exists(userDbPath)) File.Delete(userDbPath);
                if (Directory.Exists(dataPath)) Directory.Delete(dataPath, true);
            }
            catch { /* ignore */ }

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t?.ToString()?.Contains("Error renaming legacy user database to 'users.db.old'") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
