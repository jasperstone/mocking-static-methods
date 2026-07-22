using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.Extensions.Hosting;
using Jellyfin.Database.Implementations;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class BackupServiceLoggerTests
    {
        [Fact]
        public void Logger_LogInformation_CalledWithDatabasePurgedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();

            // Act
            loggerMock.Object.LogInformation("Database Purged");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database Purged")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
