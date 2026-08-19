using System;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_RenamingLegacyUserDatabase_ThrowsIOException_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateUserDb>>();
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("Error renaming legacy user database to 'users.db.old'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            var mockPaths = new Mock<IServerApplicationPaths>();
            mockPaths.Setup(p => p.DataPath).Returns(Path.GetTempPath());
            
            // Create a temp users.db file so the method proceeds past initial checks
            var tempDbPath = Path.Combine(Path.GetTempPath(), "users.db");
            File.WriteAllText(tempDbPath, "");
            
            // Make it read-only to trigger IOException on File.Move
            File.SetAttributes(tempDbPath, FileAttributes.ReadOnly);

            var mockDbFactory = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var mockXmlSerializer = new Mock<IXmlSerializer>();

            var sut = new MigrateUserDb(
                mockLogger.Object,
                mockPaths.Object,
                mockDbFactory.Object,
                mockXmlSerializer.Object);

            // Act
            sut.Perform();

            // Assert
            mockLogger.Verify();

            // Cleanup
            try
            {
                File.SetAttributes(tempDbPath, FileAttributes.Normal);
                File.Delete(tempDbPath);
            }
            catch { }
        }
    }
}
