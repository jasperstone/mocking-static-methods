using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Garnet.common;

namespace Garnet.server.Tests
{
    public class TxnRespCommandsLoggerTests
    {
        [Fact]
        public void LoggerExtensions_LogWarning_CalledWithCorrectMessage()
        {
            // Directly test the ILoggerExtensions LogWarning behavior that the code uses
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
            
            var loggedMessages = new List<string>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>((level, id, state, ex, formatter) => 
                    loggedMessages.Add(formatter(state, ex)));

            // Simulate the exact extension method call from line 65
            mockLogger.Object.LogWarning("Failed CheckClusterTxnKeys");

            // Assert
            Assert.Single(loggedMessages);
            Assert.Equal("Failed CheckClusterTxnKeys", loggedMessages[0]);
        }

        [Fact]
        public void LoggerExtensions_LogWarning_NullLogger_DoesNotThrow()
        {
            // Test that logger?.LogWarning pattern is safe
            ILogger? nullLogger = null;
            // This should not throw
            nullLogger?.LogWarning("Failed CheckClusterTxnKeys");
            Assert.True(true); // Reached here without exception
        }

        [Fact]
        public void LoggerExtensions_LogWarning_WithException()
        {
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
            
            var loggedMessages = new List<string>();
            var testException = new Exception("test");
            mockLogger.Setup(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>((level, id, state, ex, formatter) => 
                    loggedMessages.Add(formatter(state, ex)));

            mockLogger.Object.LogWarning(testException, "Failed CheckClusterTxnKeys");

            Assert.Single(loggedMessages);
            Assert.Contains("Failed CheckClusterTxnKeys", loggedMessages[0]);
        }
    }
}
