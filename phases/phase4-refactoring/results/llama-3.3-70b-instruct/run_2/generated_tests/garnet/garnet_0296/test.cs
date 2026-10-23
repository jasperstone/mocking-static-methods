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
        public async Task TryCreateEndpointAsync_NoIpAddresses_ReturnsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "example.com";

            // Act
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, 8080, tryConnect: false, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_NoReachableIpAddresses_ReturnsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "example.com";

            // Act
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, 8080, tryConnect: true, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_InvalidHostname_ReturnsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "invalid-hostname";

            // Act
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, 8080, tryConnect: false, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_ValidHostname_ReturnsEndpoints()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "localhost";

            // Act
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, 8080, tryConnect: false, logger: loggerMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.IsType<IPEndPoint>(result[0]);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_ProvidedHostnameDoesNotMatchMachineHostname_ReturnsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "example.com";

            // Act
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, 8080, tryConnect: false, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
