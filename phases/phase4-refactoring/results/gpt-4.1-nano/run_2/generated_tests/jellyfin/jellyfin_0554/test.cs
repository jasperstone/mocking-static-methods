using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Server.Implementations.Users;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Controller.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace Jellyfin.Tests.Migrations
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_Should_LogWarning_When_UserDbFileDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            string nonExistentPath = "/nonexistent/path";
            pathsMock.Setup(p => p.DataPath).Returns(nonExistentPath);
            pathsMock.Setup(p => p.UserConfigurationDirectoryPath).Returns("/some/config/path");

            var routine = new MigrateUserDb(
                loggerMock.Object,
                pathsMock.Object,
                providerMock.Object,
                xmlSerializerMock.Object);

            // Act
            routine.Perform();

            // Assert
            loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>(), It.Is<string>(s => s.Contains("users.db"))),
                Times.Once);
        }
    }
}
