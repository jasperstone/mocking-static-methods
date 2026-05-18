using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_CleanupItemsFromDeletedLibrariesStarting_UsesExtensionMethod()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            
            // Act
            loggerMock.Object.LogInformation("Starting cleanup of items from deleted libraries...");

            // Assert - Verifies the extension method LoggerExtensions.LogInformation was called
            // by checking the underlying Log method was invoked with correct parameters
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        t != null && 
                        v.ToString()!.Contains("Starting cleanup of items from deleted libraries...")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
