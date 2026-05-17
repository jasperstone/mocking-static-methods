using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class ReseedFolderFlagTests
    {
        [Fact]
        public async Task PerformAsync_LogsInformationWhenMigratingIsFolderFlag()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<object>>();
            var startupLoggerMock = new Mock<ILogger>();

            var dataPath = "DataPath";
            pathsMock.Setup(p => p.DataPath).Returns(dataPath);

            var libraryDbPath = Path.Combine(dataPath, "library.db.old");
            var dbContextMock = new Mock<object>();
            providerMock.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContextMock.Object);

            var connectionMock = new Mock<object>();
            //connectionMock.Setup(c => c.Query(It.IsAny<string>())).Returns(new List<object> { new { guid = Guid.NewGuid() } }.AsQueryable());

            var reseedFolderFlag = new ReseedFolderFlag((IStartupLogger)startupLoggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            startupLoggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task PerformAsync_LogsErrorWhenLibraryDbDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<object>>();
            var startupLoggerMock = new Mock<ILogger>();

            var dataPath = "DataPath";
            pathsMock.Setup(p => p.DataPath).Returns(dataPath);

            var libraryDbPath = Path.Combine(dataPath, "library.db.old");
            var dbContextMock = new Mock<object>();
            providerMock.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContextMock.Object);

            var connectionMock = new Mock<object>();
            //connectionMock.Setup(c => c.Query(It.IsAny<string>())).Returns(new List<object> { new { guid = Guid.NewGuid() } }.AsQueryable());

            var reseedFolderFlag = new ReseedFolderFlag((IStartupLogger)startupLoggerMock.Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            startupLoggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
