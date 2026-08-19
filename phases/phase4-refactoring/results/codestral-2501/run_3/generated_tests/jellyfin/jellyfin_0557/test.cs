using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;
using System.IO;
using MediaBrowser.Controller;
using Jellyfin.Data;
using Jellyfin.Database.Implementations;
using MediaBrowser.Model.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_WhenUserDbDoesNotExist_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            pathsMock.Setup(p => p.DataPath).Returns("non_existent_path");

            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("doesn't exist, nothing to migrate")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Perform_WhenLocalUsersv2TableDoesNotExist_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            pathsMock.Setup(p => p.DataPath).Returns("existing_path");
            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            // Mock the database connection and query to simulate the absence of the table
            var connectionMock = new Mock<SqliteConnection>();
            connectionMock.Setup(c => c.Open()).Verifiable();
            connectionMock.Setup(c => c.Query(It.IsAny<string>())).Returns(new[] { new { count = 0 } });

            providerMock.Setup(p => p.CreateDbContext()).Returns(new JellyfinDbContext());

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Table 'LocalUsersv2' doesn't exist in")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Perform_WhenIOExceptionOccurs_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            pathsMock.Setup(p => p.DataPath).Returns("existing_path");
            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            // Mock the database connection and query to simulate the presence of the table
            var connectionMock = new Mock<SqliteConnection>();
            connectionMock.Setup(c => c.Open()).Verifiable();
            connectionMock.Setup(c => c.Query(It.IsAny<string>())).Returns(new[] { new { count = 1 } });

            providerMock.Setup(p => p.CreateDbContext()).Returns(new JellyfinDbContext());

            // Mock the File.Move to throw an IOException
            var fileMock = new Mock<File>();
            fileMock.Setup(f => f.Move(It.IsAny<string>(), It.IsAny<string>())).Throws(new IOException());

            // Act
            migrateUserDb.Perform();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error renaming legacy user database to 'users.db.old'")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
