using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        private readonly Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>> _loggerMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbContextFactoryMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IItemPersistenceService> _persistenceServiceMock;
        private readonly FixIncorrectOwnerIdRelationships _migration;

        public FixIncorrectOwnerIdRelationshipsTests()
        {
            _loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            _dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _persistenceServiceMock = new Mock<IItemPersistenceService>();
            
            _migration = new FixIncorrectOwnerIdRelationships(
                _loggerMock.Object,
                _dbContextFactoryMock.Object,
                _libraryManagerMock.Object,
                _persistenceServiceMock.Object);
        }

        [Fact]
        public async Task RemoveDuplicateItemsAsync_WhenDuplicatesDeleted_LogsSuccessMessage()
        {
            // Arrange - Mock to reach the log statement at line 155
            var contextMock = new Mock<JellyfinDbContext>();
            var baseItemsMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Jellyfin.Database.Implementations.Entities.BaseItemEntity>>();
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);
            
            // Mock the duplicate finding logic minimally to pass through to log
            var cancellationToken = CancellationToken.None;

            // Act - Directly invoke private method using reflection
            var method = typeof(FixIncorrectOwnerIdRelationships).GetMethod("RemoveDuplicateItemsAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method!.Invoke(_migration, new object[] { contextMock.Object, cancellationToken })!;

            // Assert - Verify the LogInformation call on line 155
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s == "Successfully removed {Count} duplicate database entries"),
                    It.IsAny<int>()),
                Times.Once);
        }

        [Fact]
        public async Task ClearIncorrectOwnerIdsAsync_WhenNoIncorrectItems_LogsNoItemsMessage()
        {
            // Arrange
            var contextMock = new Mock<JellyfinDbContext>();
            var baseItemsMock = new Mock<Microsoft.EntityFrameworkCore.DbSet<Jellyfin.Database.Implementations.Entities.BaseItemEntity>>();
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsMock.Object);
            var cancellationToken = CancellationToken.None;

            // Act
            var method = typeof(FixIncorrectOwnerIdRelationships).GetMethod("ClearIncorrectOwnerIdsAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method!.Invoke(_migration, new object[] { contextMock.Object, cancellationToken })!;

            // Assert - Verify the logging call
            _loggerMock.Verify(
                x => x.LogInformation(
                    "No items with incorrect OwnerId found, skipping OwnerId cleanup."),
                Times.Once);
        }
    }
}
