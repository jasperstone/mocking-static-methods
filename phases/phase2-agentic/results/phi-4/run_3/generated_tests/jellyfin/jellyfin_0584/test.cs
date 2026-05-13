using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ReseedFolderFlagTests
{
    [Fact]
    public async Task PerformAsync_LogsInformationForMigratingItems()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
        var pathsMock = new Mock<IServerApplicationPaths>();
        var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();

        var dataPath = "test_data_path";
        var libraryDbPath = Path.Combine(dataPath, "library.db.old");
        var queryResult = new List<object> { new { GetGuid = (Func<int, Guid>)((index) => Guid.NewGuid()) } };

        pathsMock.Setup(p => p.DataPath).Returns(dataPath);
        File.Exists(libraryDbPath).Returns(true);

        var connectionMock = new Mock<SqliteConnection>();
        connectionMock.Setup(c => c.Query(It.IsAny<string>()))
            .Returns(queryResult);

        var dbContextMock = new Mock<JellyfinDbContext>();
        dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dbContextMock.Object);

        var sut = new ReseedFolderFlag(
            new Mock<IStartupLogger<ReseedFolderFlag>>().Object,
            dbContextFactoryMock.Object,
            pathsMock.Object);

        // Act
        await sut.PerformAsync(CancellationToken.None);

        // Assert
        loggerMock.Verify(
            l => l.LogInformation("Migrating the IsFolder flag for {Count} items.", It.Is<int>(count => count == queryResult.Count)),
            Times.Once);
    }
}
