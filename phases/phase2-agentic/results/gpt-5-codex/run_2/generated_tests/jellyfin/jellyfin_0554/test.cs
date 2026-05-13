using System;
using System.IO;
using Jellyfin.Data;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using MediaBrowser.Model.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.Server.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_WhenUserDatabaseDoesNotExist_LogsWarningAndReturns()
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDirectory);
            var userDbPath = Path.Combine(tempDirectory, "users.db");

            try
            {
                var loggerMock = new Mock<ILogger<MigrateUserDb>>();
                loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);

                var pathsMock = new Mock<IServerApplicationPaths>();
                pathsMock.SetupGet(p => p.DataPath).Returns(tempDirectory);

                var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>(MockBehavior.Strict);
                var xmlSerializerMock = new Mock<IXmlSerializer>(MockBehavior.Strict);

                var migration = new MigrateUserDb(
                    loggerMock.Object,
                    pathsMock.Object,
                    dbContextFactoryMock.Object,
                    xmlSerializerMock.Object);

                migration.Perform();

                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((value, _) =>
                            value.ToString() == $"{userDbPath} doesn't exist, nothing to migrate"),
                        It.IsAny<Exception>(),
                        (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                    Times.Once);
            }
            finally
            {
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
        }
    }
}
