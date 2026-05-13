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
        public async Task PerformAsync_LogsInformation_WhenRerunGuardFlagIsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var rerunGuardFlag = false;
            var reseedFolderFlag = new ReseedFolderFlag(new Mock<IStartupLogger<MigrateLibraryDb>>().Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task PerformAsync_LogsInformation_WhenRerunGuardFlagIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            ReseedFolderFlag.RerunGuardFlag = true;
            var reseedFolderFlag = new ReseedFolderFlag(new Mock<IStartupLogger<MigrateLibraryDb>>().Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LogsError_WhenLibraryDbDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            pathsMock.Setup(p => p.DataPath).Returns("path");
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var reseedFolderFlag = new ReseedFolderFlag(new Mock<IStartupLogger<MigrateLibraryDb>>().Object, providerMock.Object, pathsMock.Object);

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task PerformAsync_LogsInformation_WhenMigratingIsFolderFlag()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ReseedFolderFlag>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            pathsMock.Setup(p => p.DataPath).Returns("path");
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var reseedFolderFlag = new ReseedFolderFlag(new Mock<IStartupLogger<MigrateLibraryDb>>().Object, providerMock.Object, pathsMock.Object);
            var dbContextMock = new Mock<JellyfinDbContext>();
            providerMock.Setup(p => p.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContextMock.Object);
            var connectionMock = new Mock<SqliteConnection>();
            var queryResult = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Act
            await reseedFolderFlag.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }
    }
}
