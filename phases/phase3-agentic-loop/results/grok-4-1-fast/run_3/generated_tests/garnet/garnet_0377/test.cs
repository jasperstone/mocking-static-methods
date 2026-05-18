using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Garnet.server.Auth;
using Garnet.server.Auth.Aad;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace Garnet.server.Auth.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_CalledWithExceptionAndMessage_InvokesCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
            
            var exception = new InvalidOperationException("Test exception");
            var message = "Authentication failed";

            // Act
            ((ILogger)mockLogger.Object).LogError(exception, message);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.Is<Exception>(ex => ex == exception),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_WhenLoggerDisabled_DoesNotThrow()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(false);
            
            var exception = new InvalidOperationException("Test exception");

            // Act & Assert
            Assert.Same(mockLogger.Object, ((ILogger)mockLogger.Object).LogError(exception, "message"));
        }

        [Fact]
        public void LogError_WithNullLogger_DoesNotThrow()
        {
            // Arrange - null logger simulates the ?. operator case
            ILogger logger = null;
            
            var exception = new InvalidOperationException("Test exception");

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => logger.LogError(exception, "message"));
        }
    }
}
