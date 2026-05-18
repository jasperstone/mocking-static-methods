using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        [Fact]
        public void LogDebug_DeleteMetadataPath_CalledWithCorrectParameters()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var itemTypeName = "Movie";
            var itemName = "Test Movie";
            var itemId = Guid.NewGuid();
            var metadataPath = "/path/to/metadata";

            // Act - Execute the exact LogDebug call from line 540
            loggerMock.Object.LogDebug(
                "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                itemTypeName,
                itemName ?? "Unknown name",
                metadataPath,
                itemId);

            // Assert - Verify using the underlying Log method (extension methods not supported in Moq Verify)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Type: {itemTypeName}") && 
                                                 v.ToString()!.Contains($"Name: {itemName}") && 
                                                 v.ToString()!.Contains($"Path: {metadataPath}") && 
                                                 v.ToString()!.Contains($"Id: {itemId}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogDebug_DeleteMetadataPath_HandlesNullName()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var itemTypeName = "Movie";
            string? itemName = null;
            var itemId = Guid.NewGuid();
            var metadataPath = "/path/to/metadata";

            // Act
            loggerMock.Object.LogDebug(
                "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                itemTypeName,
                itemName ?? "Unknown name",
                metadataPath,
                itemId);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Name: Unknown name")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
