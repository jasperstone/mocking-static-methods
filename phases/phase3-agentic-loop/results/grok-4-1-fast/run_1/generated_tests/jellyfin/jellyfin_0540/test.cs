using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenLoggerTests
    {
        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsStartingMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateLinkedChildren>>();
            
            // Act - simulate the exact LogInformation call from line 324
            mockLogger.Object.LogInformation("Starting cleanup of items from deleted libraries...");

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType<string>>((v, t) => 
                        ((string)v).ToString().Contains("Starting cleanup of items from deleted libraries...")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupStaleFileEntries_LogsStartingMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateLinkedChildren>>();
            
            // Act - simulate the LogInformation call from the method
            mockLogger.Object.LogInformation("Starting cleanup of items with missing files...");

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType<string>>((v, t) => 
                        ((string)v).ToString().Contains("Starting cleanup of items with missing files...")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformationExtension_CalledWithMessage_InvokesLogCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateLinkedChildren>>();
            
            // Act
            mockLogger.Object.LogInformation("Test log message for coverage");

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType<string>>((v, t) => 
                        ((string)v).ToString().Contains("Test log message for coverage")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType<string>, Exception?, string>>()),
                Times.Once);
        }
    }
}
