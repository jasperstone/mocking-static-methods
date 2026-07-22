using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Logging
{
    public class LoggerExtensionsTests
    {
        private class LoggerWrapper
        {
            private readonly ILogger _logger;

            public LoggerWrapper(ILogger logger)
            {
                _logger = logger;
            }

            public virtual void LogMigrationCount(int count)
            {
                _logger.LogInformation("Migrating the IsFolder flag for {Count} items.", count);
            }
        }

        [Fact]
        public void LogMigrationCount_CallsLogInformationWithCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var wrapper = new LoggerWrapper(loggerMock.Object);
            int count = 5;

            // Act
            wrapper.LogMigrationCount(count);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Migrating the IsFolder flag for {Count} items.", count), Times.Once);
        }
    }
}
