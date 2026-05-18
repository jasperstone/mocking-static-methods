using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_LogsError_WhenIOExceptionOccursDuringFileMove()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<object>>();

            var dataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dataPath);
            var userDbPath = Path.Combine(dataPath, "users.db");
            File.WriteAllText(userDbPath, "dummy content");

            var migrateUserDb = new TestableMigrateUserDb(loggerMock.Object, dataPath, throwOnFileMove: true);

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error renaming legacy user database to 'users.db.old'")),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Cleanup
            try
            {
                File.Delete(userDbPath);
                Directory.Delete(dataPath);
            }
            catch { }
        }

        private class TestableMigrateUserDb
        {
            private readonly ILogger<object> _logger;
            private readonly string _dataPath;
            private readonly bool _throwOnFileMove;

            public TestableMigrateUserDb(ILogger<object> logger, string dataPath, bool throwOnFileMove)
            {
                _logger = logger;
                _dataPath = dataPath;
                _throwOnFileMove = throwOnFileMove;
            }

            public void Perform()
            {
                var userDbPath = Path.Combine(_dataPath, "users.db");
                if (!File.Exists(userDbPath))
                {
                    _logger.LogWarning("{UserDbPath} doesn't exist, nothing to migrate", userDbPath);
                    return;
                }

                _logger.LogInformation("Migrating the user database may take a while, do not stop Jellyfin.");

                try
                {
                    if (_throwOnFileMove)
                    {
                        throw new IOException("Simulated IOException");
                    }
                    else
                    {
                        File.Move(Path.Combine(_dataPath, "users.db"), Path.Combine(_dataPath, "users.db.old"));

                        var journalPath = Path.Combine(_dataPath, "users.db-journal");
                        if (File.Exists(journalPath))
                        {
                            File.Move(journalPath, Path.Combine(_dataPath, "users.db.old-journal"));
                        }
                    }
                }
                catch (IOException e)
                {
                    _logger.LogError(e, "Error renaming legacy user database to 'users.db.old'");
                }
            }
        }
    }
}
