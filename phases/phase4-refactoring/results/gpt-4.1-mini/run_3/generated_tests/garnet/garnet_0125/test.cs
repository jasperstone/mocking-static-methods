using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogTrace_ExtensionMethod_LogsCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var state = "STABLE";
            var nodeid = "node1";
            var slots = "1-10";

            // Act
            LoggerExtensions.LogTrace(loggerMock.Object, "Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}", state, nodeid, slots);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sending CLUSTER SETSLOTRANGE")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
