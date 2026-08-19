using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_Is_Called_With_Exception_And_Message()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var exception = new InvalidOperationException("Test exception");

            // Act
            mockLogger.Object.LogWarning(exception, "GOSSIP round faulted");

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GOSSIP round faulted")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
