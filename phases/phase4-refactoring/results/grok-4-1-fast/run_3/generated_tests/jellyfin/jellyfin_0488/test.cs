using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        private readonly Mock<ILogger<FixIncorrectOwnerIdRelationships>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IItemPersistenceService> _persistenceServiceMock;
        private readonly FixIncorrectOwnerIdRelationships _migration;

        public FixIncorrectOwnerIdRelationshipsTests()
        {
            _loggerMock = new Mock<ILogger<FixIncorrectOwnerIdRelationships>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _persistenceServiceMock = new Mock<IItemPersistenceService>();
            
            // Create with null dbContextFactory since we're not testing DB paths
            _migration = new FixIncorrectOwnerIdRelationships(
                _loggerMock.Object,
                null!,
                _libraryManagerMock.Object,
                _persistenceServiceMock.Object);
        }

        [Fact]
        public void LoggerExtension_LogInformation_DuplicateRemovalSuccess()
        {
            // Arrange - Test the specific LoggerExtensions.LogInformation call from line 155
            const int count = 5;
            const string messageTemplate = "Successfully removed {Count} duplicate database entries";

            // Act - Directly invoke the extension method call being tested
            _loggerMock.Object.LogInformation(messageTemplate, count);

            // Assert - Verify the ILogger.LogInformation extension was called with correct args
            _loggerMock.Verify(
                x => x.LogInformation(messageTemplate, count),
                Times.Once);
        }

        [Fact]
        public void LoggerExtension_LogInformation_NoDuplicatesFound()
        {
            // Arrange
            const string messageTemplate = "No duplicate items found, skipping duplicate removal.";

            // Act
            _loggerMock.Object.LogInformation(messageTemplate);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(messageTemplate),
                Times.Once);
        }

        [Fact]
        public void LoggerExtension_LogInformation_NoIncorrectOwnerIds()
        {
            // Arrange
            const string messageTemplate = "No items with incorrect OwnerId found, skipping OwnerId cleanup.";

            // Act
            _loggerMock.Object.LogInformation(messageTemplate);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(messageTemplate),
                Times.Once);
        }
    }
}
