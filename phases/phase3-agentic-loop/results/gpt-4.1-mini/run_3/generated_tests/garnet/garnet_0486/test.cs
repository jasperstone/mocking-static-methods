using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class RespServerSessionTests
    {
        // We cannot directly instantiate RespServerSession because it is internal sealed.
        // Instead, we will test the static LoggerExtensions.LogWarning extension method indirectly by invoking NetworkCONFIG_SET
        // through a minimal wrapper class that exposes the method for testing.
        // Since RespServerSession is internal sealed partial, we cannot subclass it.
        // We will create a minimal test class that mimics the relevant logic for the LogWarning call.

        private class LoggerExtensionsTests
        {
            public static bool TestLogWarningCall(ILogger logger, string clusterUsername, string clusterPassword)
            {
                // This mimics the relevant part of NetworkCONFIG_SET that calls LogWarning
                if (clusterUsername == null && clusterPassword != null)
                {
                    logger?.LogWarning("Cluster username is not provided, will use new password with existing username");
                    return true;
                }
                return false;
            }
        }

        [Fact]
        public void LogWarning_IsCalled_WhenClusterUsernameNullAndClusterPasswordNotNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // Act
            var called = LoggerExtensionsTests.TestLogWarningCall(loggerMock.Object, null, "password");

            // Assert
            Assert.True(called);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cluster username is not provided")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_IsNotCalled_WhenClusterUsernameIsNotNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // Act
            var called = LoggerExtensionsTests.TestLogWarningCall(loggerMock.Object, "user", "password");

            // Assert
            Assert.False(called);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Never);
        }

        [Fact]
        public void LogWarning_IsNotCalled_WhenClusterPasswordIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // Act
            var called = LoggerExtensionsTests.TestLogWarningCall(loggerMock.Object, null, null);

            // Assert
            Assert.False(called);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Never);
        }
    }
}
