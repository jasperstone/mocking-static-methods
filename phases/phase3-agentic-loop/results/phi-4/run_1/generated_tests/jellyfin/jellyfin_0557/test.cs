using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void LogError_ShouldBeCalled_WhenIOExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var dataPath = "testPath";
            var dbFilename = "users.db";
            var exception = new IOException("Test exception");

            // Act
            try
            {
                File.Move(Path.Combine(dataPath, dbFilename), Path.Combine(dataPath, dbFilename + ".old"));
                File.Move(Path.Combine(dataPath, dbFilename + "-journal"), Path.Combine(dataPath, dbFilename + ".old-journal"));
            }
            catch (IOException e)
            {
                loggerMock.Object.LogError(e, "Error renaming legacy user database to 'users.db.old'");
            }

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Error renaming legacy user database to 'users.db.old'",
                    It.Is<object[]>(o => o.Length == 0)),
                Times.Once);
        }
    }
}
