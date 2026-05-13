using Xunit;
using Moq;
using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Garnet.common
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_NoIpAddresses_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "example.com";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 1234, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), hostname), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_NoReachableIpAddresses_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "example.com";
            var ipAddresses = new[] { IPAddress.Parse("192.168.1.1") };

            // Act
            var result = Format.TryCreateEndpoint(hostname, 1234, tryConnect: true, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), hostname), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_InvalidHostname_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "invalid-hostname";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 1234, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), hostname), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_ValidHostname_ReturnsEndpoints()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "localhost";
            var port = 1234;

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, logger: loggerMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(port, ((IPEndPoint)result[0]).Port);
        }
    }
}
