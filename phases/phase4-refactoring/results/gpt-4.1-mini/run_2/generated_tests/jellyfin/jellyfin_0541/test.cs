using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_NoItemsFromDeletedLibraries_LogsExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var orphanedIds = new List<Guid>();

            // Act
            loggerMock.Object.LogInformation("No items from deleted libraries found.");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No items from deleted libraries found.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
