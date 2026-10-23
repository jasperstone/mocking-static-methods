using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogTrace_CallsILoggerLogWithCorrectParameters()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var state = "IMPORT";
            var nodeid = "node1";
            var slots = "0-100";

            // Act
            loggerMock.Object.LogTrace("Sending CLUSTER SETSLOTRANGE {state} {nodeid} {slots}", state, nodeid, slots);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains("Sending CLUSTER SETSLOTRANGE") &&
                    v.ToString().Contains(state) &&
                    v.ToString().Contains(nodeid) &&
                    v.ToString().Contains(slots)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
