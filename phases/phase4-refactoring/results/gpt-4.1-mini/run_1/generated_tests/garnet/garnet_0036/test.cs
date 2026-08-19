using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.Cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogTrace_IsCalled_WithExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int slot = 5;
            string nodeId = "node123";

            // Act
            loggerMock.Object.LogTrace("[Processed] SetSlot {slot} FORCED TO {nodeId}", slot, nodeId);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("[Processed] SetSlot")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
