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

            // Act
            var result = Format.TryCreateEndpoint(singleAddressOrHostname, 8080, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_NoReachableIpAddresses_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "example.com";
            var ipAddresses = new IPAddress[] { IPAddress.Parse("192.168.1.1") };

            // Act
            var result = Format.TryCreateEndpoint(singleAddressOrHostname, 8080, tryConnect: true, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_InvalidHostname_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "invalid-hostname";

            // Act
            var result = Format.TryCreateEndpoint(singleAddressOrHostname, 8080, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.Null(result);
        }
    }
}
