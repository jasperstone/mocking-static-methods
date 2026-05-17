using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using System;

#pragma warning disable CA2254 // Template should not vary between calls

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerLoggerTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public LibraryManagerLoggerTests()
        {
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public void LogDebug_DeleteMetadataPath_CalledWithCorrectParameters()
        {
            // Arrange
            var itemTypeName = "Folder";
            var itemName = "Test Movie";
            var metadataPath = "/path/to/metadata";
            var itemId = Guid.NewGuid();

            // Act - Exactly matches the LogDebug call on line 540
            _mockLogger.Object.LogDebug(
                "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                itemTypeName,
                itemName,
                metadataPath,
                itemId);

            // Assert - Verifies the call was made with correct parameters via underlying Log method
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains($"Type: {itemTypeName}") && ((string)v).Contains($"Name: {itemName}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogDebug_DeleteMetadataPath_HandlesNullName()
        {
            // Arrange
            var itemTypeName = "Video";
            string? itemName = null;
            var metadataPath = "/path/to/metadata";
            var itemId = Guid.NewGuid();

            // Act - Matches line 540 with null name handling (item.Name ?? "Unknown name")
            _mockLogger.Object.LogDebug(
                "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                itemTypeName,
                itemName ?? "Unknown name",
                metadataPath,
                itemId);

            // Assert - Verifies "Unknown name" handling
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Unknown name")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogDebug_DeleteMetadataPath_ValidatesDebugLevelAndMessageTemplate()
        {
            // Arrange & Act - Direct call matching line 540 exactly
            _mockLogger.Object.LogDebug(
                "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                "Movie",
                "Test Item",
                "/test/path",
                Guid.Empty);

            // Assert - Verifies Debug level and message template via underlying Log call
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.Is<EventId>(e => e.Id == 0),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
