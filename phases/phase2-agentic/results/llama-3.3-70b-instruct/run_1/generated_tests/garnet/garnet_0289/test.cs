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
            var port = 1234;

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
            var port = 1234;

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
            var port = 1234;

            // Act
            var result = Format.TryCreateEndpoint(singleAddressOrHostname, port, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError("Error while trying to resolve hostname: {exMessage} [{hostname}]", It.IsAny<string>(), singleAddressOrHostname), Times.Once);
            Assert.Null(result);
        }
    }
}
