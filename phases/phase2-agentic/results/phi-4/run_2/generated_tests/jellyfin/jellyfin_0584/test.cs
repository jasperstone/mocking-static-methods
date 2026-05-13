using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Data;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.ServerSetupApp;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class ReseedFolderFlagTests
    {
        [Fact]
        public async Task PerformAsync_LogsInformationForMigratingItems()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var cancellationToken = CancellationToken.None;

            var dataPath = "test_data_path";
            var libraryDbPath = Path.Combine(dataPath, "library.db.old");
            var queryResult = new List<object> { new { GetGuid = (Func<int, Guid>)((index) => Guid.NewGuid()) } };

            pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            File.Exists(libraryDbPath).Returns(true);

            var dbContextMock = new Mock<JellyfinDbContext>();
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dbContextMock.Object);

            var connectionMock = new Mock<SqliteConnection>(new SqliteConnectionStringBuilder { DataSource = libraryDbPath }.ConnectionString);
            connectionMock.Setup(c => c.Query<object>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<SqliteCommandBehavior>()))
                .Returns(queryResult);

            var sut = new ReseedFolderFlag(
                new Mock<IStartupLogger<ReseedFolderFlag>>().Object,
                dbContextFactoryMock.Object,
                pathsMock.Object);

            // Act
            await sut.PerformAsync(cancellationToken);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.Is<string>(s => s.Contains("Migrating the IsFolder flag for {Count} items.")),
                    It.Is<int>(count => count == queryResult.Count)),
                Times.Once);
        }
    }
}
