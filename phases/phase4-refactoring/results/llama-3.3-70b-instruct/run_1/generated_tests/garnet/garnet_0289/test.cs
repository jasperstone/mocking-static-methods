using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.common
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_NoIpAddresses_ReturnsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "non-existent-hostname";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 8080, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError("No IP address found for hostname:{hostname}", hostname), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_NoReachableIpAddresses_ReturnsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "localhost";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 8080, tryConnect: true, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError("No reachable IP address found for hostname:{hostname}", hostname), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_InvalidHostname_ReturnsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "-invalid-hostname";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 8080, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_ValidHostname_ReturnsEndpoints()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "localhost";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 8080, logger: loggerMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }
    }
}
