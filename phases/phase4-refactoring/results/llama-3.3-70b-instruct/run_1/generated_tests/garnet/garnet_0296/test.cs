using Xunit;
using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
            var port = 8080;

            // Act
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, port, tryConnect: false, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError("No IP address found for hostname:{hostname}", singleAddressOrHostname), Times.Once);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_NoReachableIpAddresses_ReturnsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "example.com";
            var port = 8080;

            // Act
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, port, tryConnect: true, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError("No reachable IP address found for hostname:{hostname}", singleAddressOrHostname), Times.Once);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_HostnameDoesNotMatchMachineHostname_ReturnsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "example.com";
            var port = 8080;

            // Act
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, port, tryConnect: false, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError("Provided hostname does not much acquired machine name {addressOrHostname} {machineHostname}!", singleAddressOrHostname, Environment.MachineName), Times.Once);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_InvalidHostname_ReturnsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "invalid-hostname";
            var port = 8080;

            // Act
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, port, tryConnect: false, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError("No IP address found for hostname:{hostname}", singleAddressOrHostname), Times.Once);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_ValidHostname_ReturnsEndpoint()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleAddressOrHostname = "localhost";
            var port = 8080;

            // Act
            var result = await Format.TryCreateEndpointAsync(singleAddressOrHostname, port, tryConnect: false, logger: loggerMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.IsType<IPEndPoint>(result[0]);
        }
    }
}
