using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.common
{
    public class FormatTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public FormatTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void TryCreateEndpoint_NoIpAddressesFound_LogsError()
        {
            // Arrange
            string invalidHostname = "nonexistent.invalid";
            _loggerMock.Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("No IP address found for hostname:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Act
            var result = Format.TryCreateEndpoint(invalidHostname, 6379, logger: _loggerMock.Object);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("No IP address found for hostname:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameMismatch_LogsError()
        {
            // Arrange
            string hostname = "wronghost";
            int port = 6379;
            _loggerMock.Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("Provided hostname does not much acquired machine name")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, tryConnect: false, logger: _loggerMock.Object);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("Provided hostname does not much acquired machine name")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_TryConnectNoReachableIps_LogsError()
        {
            // Arrange
            string hostname = "localhost";
            int port = 9999; // Unlikely to have anything listening
            _loggerMock.Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("No reachable IP address found for hostname:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, tryConnect: true, logger: _loggerMock.Object);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("No reachable IP address found for hostname:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_DnsResolutionException_LogsError()
        {
            // Arrange
            string invalidDns = "\0invalid"; // Null char should cause DNS exception
            _loggerMock.Setup(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("Error while trying to resolve hostname:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Act
            var result = Format.TryCreateEndpoint(invalidDns, 6379, logger: _loggerMock.Object);

            // Assert
            Assert.Null(result);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("Error while trying to resolve hostname:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_ValidIpAddress_NoLogging()
        {
            // Arrange
            string validIp = "127.0.0.1";

            // Act
            var result = Format.TryCreateEndpoint(validIp, 6379, logger: _loggerMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }
    }
}
