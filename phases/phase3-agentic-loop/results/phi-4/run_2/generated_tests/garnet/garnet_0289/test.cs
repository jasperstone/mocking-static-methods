using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;

namespace GarnetTests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_NoIPAddressesFound_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            string hostname = "nonexistent.example.com";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 80, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("No IP address found for hostname:")), hostname),
                Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameDoesNotMatchMachineName_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            string hostname = "different.example.com";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 80, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Provided hostname does not much acquired machine name")), hostname, It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_NoReachableIPAddresses_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            string hostname = "localhost";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 80, tryConnect: true, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("No reachable IP address found for hostname:")), hostname),
                Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_ExceptionWhileResolvingHostname_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            string hostname = "invalid.hostname";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 80, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                l => l.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Error while trying to resolve hostname:")), It.IsAny<string>(), hostname),
                Times.Once);
        }
    }
}
