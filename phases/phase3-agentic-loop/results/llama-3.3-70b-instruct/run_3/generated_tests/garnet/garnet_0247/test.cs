using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster
{
    public class LoggerTests
    {
        [Fact]
        public void LogWarning_Called_When_Exception_Occurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new Exception("Test exception");

            // Act
            loggerMock.Object.LogWarning(exception, "An exception occurred at ReplicationManager.ProcessPrimaryStream");

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<EventId>(), It.IsAny<string>(), exception, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
