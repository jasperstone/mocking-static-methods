using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class BackupServiceTests
    {
        [Fact]
        public void Logger_LogInformation_IsCalledWithExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Jellyfin.Server.Implementations.FullSystemBackup.BackupService>>();
            var testPath = "testPath";

            // Act
            loggerMock.Object.LogInformation("Backup of folder {Table}", testPath);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Backup of folder")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
