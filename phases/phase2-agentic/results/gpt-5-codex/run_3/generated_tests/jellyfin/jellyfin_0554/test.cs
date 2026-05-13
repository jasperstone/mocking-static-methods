using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using MediaBrowser.Model.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_WhenUserDatabaseIsMissing_LogsWarningWithUserDbPath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();

            var tempDataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDataDirectory);

            pathsMock.SetupGet(p => p.DataPath).Returns(tempDataDirectory);

            var dbContextFactory = Mock.Of<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializer = Mock.Of<IXmlSerializer>();

            var migration = new MigrateUserDb(loggerMock.Object, pathsMock.Object, dbContextFactory, xmlSerializer);

            var expectedUserDbPath = Path.Combine(tempDataDirectory, "users.db");

            try
            {
                // Act
                migration.Perform();

                // Assert
                var invocation = Assert.Single(loggerMock.Invocations);

                Assert.Equal(LogLevel.Warning, (LogLevel)invocation.Arguments[0]);
                var eventId = (EventId)invocation.Arguments[1];
                Assert.Equal(0, eventId.Id);

                var state = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object>>>(invocation.Arguments[2]);
                var stateDictionary = state.ToDictionary(kv => kv.Key, kv => kv.Value);

                Assert.Equal(expectedUserDbPath, stateDictionary["UserDbPath"]);
                Assert.Equal("{UserDbPath} doesn't exist, nothing to migrate", stateDictionary["{OriginalFormat}"]);

                Assert.Null(invocation.Arguments[3]);
            }
            finally
            {
                if (Directory.Exists(tempDataDirectory))
                {
                    Directory.Delete(tempDataDirectory, true);
                }
            }
        }
    }
}
