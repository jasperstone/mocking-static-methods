using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
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
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, port, tryConnect: false, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_HostnameDoesNotMatchMachineName_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "example.com";
            var port = 1234;

            // Act
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, port, tryConnect: false, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
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
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
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
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, port, tryConnect: false, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.Null(result);
        }
    }
}
