using Xunit;
using Moq;
using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_NoIpAddresses_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "example.com";
            var port = 8080;

            // Act
            var result = Format.TryCreateEndpoint(singleAddressOrHostname, port, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError("No IP address found for hostname:{hostname}", singleAddressOrHostname), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_NoReachableIpAddresses_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "example.com";
            var port = 8080;

            // Act
            var result = Format.TryCreateEndpoint(singleAddressOrHostname, port, tryConnect: true, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError("No reachable IP address found for hostname:{hostname}", singleAddressOrHostname), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_InvalidHostname_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "invalid-hostname";
            var port = 8080;

            // Act
            var result = Format.TryCreateEndpoint(singleAddressOrHostname, port, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError("No IP address found for hostname:{hostname}", singleAddressOrHostname), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_ValidIpAddress_ReturnsEndpoint()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "127.0.0.1";
            var port = 8080;

            // Act
            var result = Format.TryCreateEndpoint(singleAddressOrHostname, port, logger: loggerMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(IPAddress.Loopback, ((IPEndPoint)result[0]).Address);
            Assert.Equal(port, ((IPEndPoint)result[0]).Port);
        }

        [Fact]
        public void TryCreateEndpoint_ValidHostname_ReturnsEndpoint()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "localhost";
            var port = 8080;

            // Act
            var result = Format.TryCreateEndpoint(singleAddressOrHostname, port, logger: loggerMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(IPAddress.Loopback, ((IPEndPoint)result[0]).Address);
            Assert.Equal(port, ((IPEndPoint)result[0]).Port);
        }
    }
}
