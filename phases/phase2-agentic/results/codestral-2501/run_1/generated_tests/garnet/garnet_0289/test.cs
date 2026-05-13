using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_InvalidHostname_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "invalidhostname";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No IP address found for hostname")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_ValidHostname_ReturnsEndpoints()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "localhost";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 8080, false, loggerMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Length);
            Assert.Equal(IPAddress.Loopback, ((IPEndPoint)result[0]).Address);
        }

        [Fact]
        public void TryCreateEndpoint_ValidIPAddress_ReturnsEndpoints()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var ipAddress = "127.0.0.1";

            // Act
            var result = Format.TryCreateEndpoint(ipAddress, 8080, false, loggerMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Length);
            Assert.Equal(IPAddress.Parse(ipAddress), ((IPEndPoint)result[0]).Address);
        }

        [Fact]
        public void TryCreateEndpoint_InvalidHostname_LogsError_WhenTryConnectIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "invalidhostname";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 8080, true, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No IP address found for hostname")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_ValidHostname_LogsError_WhenTryConnectIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "localhost";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 8080, true, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No reachable IP address found for hostname")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_ValidIPAddress_LogsError_WhenTryConnectIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var ipAddress = "127.0.0.1";

            // Act
            var result = Format.TryCreateEndpoint(ipAddress, 8080, true, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No reachable IP address found for hostname")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Null(result);
        }
    }
}
