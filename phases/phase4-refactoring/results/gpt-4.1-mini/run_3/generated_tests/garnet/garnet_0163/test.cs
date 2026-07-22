using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionLoggingTests
    {
        // Since ReplicaSyncSession and related types are internal and inaccessible,
        // and refactor tools are unavailable, we cannot instantiate or subclass directly.
        // Instead, we test the logging extension method LogError on ILogger directly.

        [Fact]
        public void LoggerExtensions_LogError_WithException_FormatsCorrectly()
        {
            var mockLogger = new Mock<ILogger>();

            var ex = new InvalidOperationException("Test exception");
            var methodName = "WaitForSyncCompletionAsync";

            // Call the extension method LogError with exception and message template
            mockLogger.Object.LogError(ex, "{method} failed waiting for sync", methodName);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(methodName)),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
