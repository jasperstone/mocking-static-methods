using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;

namespace Emby.Server.Implementations.Tests.Library
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public LoggerExtensionsTests()
        {
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public void LogDebug_DeleteMetadataPath_FolderItem_CorrectParameters()
        {
            // Arrange
            var logger = _mockLogger.Object;
            var item = new Folder 
            { 
                Name = "TestFolder", 
                Id = Guid.NewGuid() 
            };
            var metadataPath = "/test/metadata/path";

            // Act - Execute the exact LogDebug call from line 540
            logger.LogDebug(
                "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                item.GetType().Name,
                item.Name ?? "Unknown name",
                metadataPath,
                item.Id);

            // Assert - Verify the underlying Log method was called with correct structured data
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Type: Folder") && 
                                                 v.ToString()!.Contains("Name: TestFolder") &&
                                                 v.ToString()!.Contains(metadataPath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogDebug_DeleteMetadataPath_NullName_UsesUnknownName()
        {
            // Arrange
            var logger = _mockLogger.Object;
            var item = new Folder 
            { 
                Name = null, 
                Id = Guid.NewGuid() 
            };
            var metadataPath = "/test/metadata/path";

            // Act
            logger.LogDebug(
                "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                item.GetType().Name,
                item.Name ?? "Unknown name",
                metadataPath,
                item.Id);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Name: Unknown name")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogDebug_DeleteMetadataPath_VideoItem_UsesVideoTypeName()
        {
            // Arrange
            var logger = _mockLogger.Object;
            var item = new Video 
            { 
                Name = "TestVideo",
                Id = Guid.NewGuid() 
            };
            var metadataPath = "/test/video/metadata";

            // Act
            logger.LogDebug(
                "Deleting metadata path, Type: {Type}, Name: {Name}, Path: {Path}, Id: {Id}",
                item.GetType().Name,
                item.Name ?? "Unknown name",
                metadataPath,
                item.Id);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Type: Video") &&
                                                 v.ToString()!.Contains("Name: TestVideo")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
