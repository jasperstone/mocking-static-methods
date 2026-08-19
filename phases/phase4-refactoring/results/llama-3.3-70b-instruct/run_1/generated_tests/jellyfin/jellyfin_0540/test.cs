using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using Jellyfin.Server.Migrations.Routines;

namespace Jellyfin.Server.Tests
{
    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void LogInformation_CalledWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            var dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var migrateLinkedChildren = new MigrateLinkedChildren(loggerMock.Object, dbProviderMock.Object, libraryManagerMock.Object, appHostMock.Object, appPathsMock.Object);

            // Act
            migrateLinkedChildren.CleanupItemsFromDeletedLibraries(new JellyfinDbContext());

            // Assert
            loggerMock.Verify(l => l.LogInformation("Starting cleanup of items from deleted libraries..."), Times.Once);
        }
    }
}
