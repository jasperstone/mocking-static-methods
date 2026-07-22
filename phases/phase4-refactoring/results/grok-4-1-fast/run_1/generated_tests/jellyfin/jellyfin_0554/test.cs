using System;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_WhenUserDbDoesNotExist_LogsWarningAndReturns()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateUserDb>>();
            var mockPaths = new Mock<IServerApplicationPaths>();
            mockPaths.Setup(p => p.DataPath).Returns("/test/data");
            
            var mockProvider = new Mock<object>();
            var mockSerializer = new Mock<IXmlSerializer>();

            // Mock File.Exists using a shim-like approach
            var userDbPath = Path.Combine("/test/data", "users.db");
            var fileExistsMock = new Mock<Func<string, bool>>();
            fileExistsMock.Setup(f => f(userDbPath)).Returns(false);

            var migrator = new MigrateUserDb(
                mockLogger.Object,
                mockPaths.Object,
                (dynamic)mockProvider.Object,
                mockSerializer.Object);

            // Act
            migrator.Perform();

            // Assert - verify the LogWarning extension method call
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v?.ToString()!.Contains("{UserDbPath} doesn't exist, nothing to migrate") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
