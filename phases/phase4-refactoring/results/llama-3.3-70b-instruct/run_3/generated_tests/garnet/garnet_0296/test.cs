using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        [Fact]
        public async Task TryCreateEndpointAsync_NoIpAddresses_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "example.com";
            var port = 1234;

            // Act
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, port, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), singleAddressOrHostname), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_HostnameDoesNotMatchMachineName_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "example.com";
            var port = 1234;
            var machineHostname = "localhost";

            // Act
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, port, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), singleAddressOrHostname, machineHostname), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_NoReachableIpAddresses_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "example.com";
            var port = 1234;

            // Act
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, port, tryConnect: true, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), singleAddressOrHostname), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_ResolutionError_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "example.com";
            var port = 1234;

            // Act
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, port, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<Exception>().Message, singleAddressOrHostname), Times.Once);
            Assert.Null(result);
        }
    }
}
