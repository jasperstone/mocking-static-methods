using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ClusterManagerLogTraceTests
    {
        [Fact]
        public void LogTrace_IsCalled_When_SetSlot()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()));

            // Act
            mockLogger.Object.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", 5, "node2");

            // Assert
            mockLogger.Verify(x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("[Processed] SetSlot") && v.ToString().Contains("5") && v.ToString().Contains("node2")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
