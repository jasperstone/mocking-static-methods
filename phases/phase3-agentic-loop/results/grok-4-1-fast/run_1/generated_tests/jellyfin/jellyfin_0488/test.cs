using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Server.ServerSetupApp;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        private readonly Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IItemPersistenceService> _persistenceServiceMock;
        private readonly FixIncorrectOwnerIdRelationships _migration;

        public FixIncorrectOwnerIdRelationshipsTests()
        {
            _loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _persistenceServiceMock = new Mock<IItemPersistenceService>();
            
            // Setup logger to support ILogger extension methods
            _loggerMock.As<ILogger<FixIncorrectOwnerIdRelationships>>();

            _migration = new FixIncorrectOwnerIdRelationships(
                _loggerMock.Object,
                Mock.Of<IDbContextFactory<Jellyfin.Database.Implementations.JellyfinDbContext>>(),
                _libraryManagerMock.Object,
                _persistenceServiceMock.Object);
        }

        [Fact]
        public async Task RemoveDuplicateItemsAsync_WhenDuplicatesDeleted_LogsSuccessMessage()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var contextMock = new Mock<Jellyfin.Database.Implementations.JellyfinDbContext>(
                new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Jellyfin.Database.Implementations.JellyfinDbContext>().Options);

            // Act
            await InvokePrivateMethodAsync("RemoveDuplicateItemsAsync", contextMock.Object, cancellationToken);

            // Assert - Verify the specific LogInformation call (line 155)
            _loggerMock.Verify(
                l => l.LogInformation("Successfully removed {Count} duplicate database entries", It.IsAny<int>()),
                Times.Once);
        }

        [Fact]
        public async Task RemoveDuplicateItemsAsync_WhenNoDuplicatesFound_LogsNoDuplicatesMessage()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var contextMock = new Mock<Jellyfin.Database.Implementations.JellyfinDbContext>(
                new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Jellyfin.Database.Implementations.JellyfinDbContext>().Options);

            // Act
            await InvokePrivateMethodAsync("RemoveDuplicateItemsAsync", contextMock.Object, cancellationToken);

            // Assert
            _loggerMock.Verify(
                l => l.LogInformation("No duplicate items found, skipping duplicate removal."),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ClearIncorrectOwnerIdsAsync_WhenNoIncorrectItemsFound_LogsNoItemsMessage()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var contextMock = new Mock<Jellyfin.Database.Implementations.JellyfinDbContext>(
                new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Jellyfin.Database.Implementations.JellyfinDbContext>().Options);

            // Act
            await InvokePrivateMethodAsync("ClearIncorrectOwnerIdsAsync", contextMock.Object, cancellationToken);

            // Assert - Verifies ILogger extension usage similar to line 155
            _loggerMock.Verify(
                l => l.LogInformation("No items with incorrect OwnerId found, skipping OwnerId cleanup."),
                Times.Once);
        }

        private async Task InvokePrivateMethodAsync(string methodName, params object[] parameters)
        {
            var method = typeof(FixIncorrectOwnerIdRelationships)
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!;
            await (Task)method.Invoke(_migration, parameters)!;
        }
    }
}
