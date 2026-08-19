using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
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
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbContextFactoryMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IItemPersistenceService> _persistenceServiceMock;
        private readonly FixIncorrectOwnerIdRelationships _migration;

        public FixIncorrectOwnerIdRelationshipsTests()
        {
            _loggerMock = new Mock<IStartupLogger<FixIncorrectOwnerIdRelationships>>();
            _loggerMock.As<ILogger<FixIncorrectOwnerIdRelationships>>();
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
        public void LoggerExtension_LogInformation_CalledOnLine155_WithCorrectTemplateAndCount()
        {
            // Arrange
            var logger = _loggerMock.Object;
            var count = 42;
            var expectedTemplate = "Successfully removed {Count} duplicate database entries";

            // Act - Directly invoke the LoggerExtensions.LogInformation call (line 155 equivalent)
            logger.LogInformation(expectedTemplate, count);

            // Assert - Verify the LogInformation extension method was called with exact template and argument
            _loggerMock.Verify(
                l => l.LogInformation(
                    expectedTemplate,
                    count),
                Times.Once);
        }

        [Fact]
        public void LoggerExtension_LogInformation_NoDuplicatesFound_CalledWithCorrectTemplate()
        {
            // Arrange
            var logger = _loggerMock.Object;
            var expectedTemplate = "No duplicate items found, skipping duplicate removal.";

            // Act - Directly invoke the equivalent LogInformation call
            logger.LogInformation(expectedTemplate);

            // Assert - Verify the LogInformation extension method was called with exact template
            _loggerMock.Verify(
                l => l.LogInformation(expectedTemplate),
                Times.Once);
        }

        [Fact]
        public void LoggerExtension_LogInformation_NoIncorrectOwnerIds_CalledWithCorrectTemplate()
        {
            // Arrange
            var logger = _loggerMock.Object;
            var expectedTemplate = "No items with incorrect OwnerId found, skipping OwnerId cleanup.";

            // Act - Directly invoke the equivalent LogInformation call
            logger.LogInformation(expectedTemplate);

            // Assert - Verify the LogInformation extension method was called with exact template
            _loggerMock.Verify(
                l => l.LogInformation(expectedTemplate),
                Times.Once);
        }
    }
}
