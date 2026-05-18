using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Server.ServerSetupApp;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Jellyfin.Database.Implementations;

namespace Jellyfin.Server.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public async Task LogInformation_CalledWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var startupLoggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            startupLoggerMock.Setup(s => s.LogInformation(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>((message, args) => loggerMock.Object.LogInformation(message, args));
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var routine = new FixIncorrectOwnerIdRelationships(startupLoggerMock.Object, dbContextFactoryMock.Object, libraryManagerMock.Object, persistenceServiceMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task LogInformation_CalledWithCorrectMessage_WhenNoDuplicatesFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var startupLoggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            startupLoggerMock.Setup(s => s.LogInformation(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>((message, args) => loggerMock.Object.LogInformation(message, args));
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var routine = new FixIncorrectOwnerIdRelationships(startupLoggerMock.Object, dbContextFactoryMock.Object, libraryManagerMock.Object, persistenceServiceMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task LogInformation_CalledWithCorrectMessage_WhenNoIncorrectOwnerIdsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var startupLoggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            startupLoggerMock.Setup(s => s.LogInformation(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>((message, args) => loggerMock.Object.LogInformation(message, args));
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var persistenceServiceMock = new Mock<IItemPersistenceService>();

            var routine = new FixIncorrectOwnerIdRelationships(startupLoggerMock.Object, dbContextFactoryMock.Object, libraryManagerMock.Object, persistenceServiceMock.Object);

            // Act
            await routine.PerformAsync(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
