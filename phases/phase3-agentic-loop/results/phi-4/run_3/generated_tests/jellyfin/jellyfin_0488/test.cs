using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests
{
    public class FixIncorrectOwnerIdRelationshipsTests
    {
        [Fact]
        public void LogInformation_ShouldBeCalled_WhenAllIdsToDeleteCountIsGreaterThanZero()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var allIdsToDelete = new List<string> { "1", "2", "3" };

            var sut = new FixIncorrectOwnerIdRelationships(loggerMock.Object);

            // Act
            sut.RemoveDuplicateEntries(allIdsToDelete);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Successfully removed {Count} duplicate database entries", StringComparison.Ordinal) &&
                        ((object[])v)[0] is int count && count == allIdsToDelete.Count),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    public class FixIncorrectOwnerIdRelationships
    {
        private readonly ILogger _logger;

        public FixIncorrectOwnerIdRelationships(ILogger logger)
        {
            _logger = logger;
        }

        public void RemoveDuplicateEntries(List<string> allIdsToDelete)
        {
            if (allIdsToDelete.Count > 0)
            {
                _logger.LogInformation("Successfully removed {Count} duplicate database entries", allIdsToDelete.Count);
            }
        }
    }
}
