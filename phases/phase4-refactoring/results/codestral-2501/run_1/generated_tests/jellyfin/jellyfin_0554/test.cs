using System;
using System.Collections.Generic;
using System.IO;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Data;
using Microsoft.EntityFrameworkCore;
using MediaBrowser.Model.Serialization;
using Jellyfin.Database.Implementations;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_WhenUserDbDoesNotExist_LogsWarning()
        {
            // Arrange
            var logMessages = new List<string>();
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Callback<LogLevel, EventId, It.IsAnyType, Exception, Func<It.IsAnyType, Exception, string>>(
                    (logLevel, eventId, state, exception, formatter) =>
                    {
                        logMessages.Add(formatter(state, exception));
                    });

            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            var dataPath = "path/to/data";
            var userDbPath = Path.Combine(dataPath, "users.db");

            pathsMock.Setup(p => p.DataPath).Returns(dataPath);

            var migrateUserDb = new MigrateUserDb(
                loggerMock.Object,
                pathsMock.Object,
                providerMock.Object,
                xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            Assert.Contains($"{userDbPath} doesn't exist, nothing to migrate", logMessages);
        }
    }
}
