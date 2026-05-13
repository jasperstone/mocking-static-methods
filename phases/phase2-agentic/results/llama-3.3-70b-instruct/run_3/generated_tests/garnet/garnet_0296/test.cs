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
            var hostname = "example.com";
            var port = 1234;

            // Act
            var result = await Format.TryCreateEndpointAsync(hostname, port, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError("No IP address found for hostname:{hostname}", hostname), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_NoReachableIpAddresses_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "example.com";
            var port = 1234;
            var ipAddresses = new[] { IPAddress.Parse("192.168.1.1") };

            // Act
            var result = await Format.TryCreateEndpointAsync(hostname, port, tryConnect: true, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError("No reachable IP address found for hostname:{hostname}", hostname), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_HostnameDoesNotMatchMachineHostname_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "example.com";
            var port = 1234;
            var ipAddresses = new[] { IPAddress.Parse("192.168.1.1") };
            var machineHostname = "machine.example.com";

            // Act
            var result = await Format.TryCreateEndpointAsync(hostname, port, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError("Provided hostname does not much acquired machine name {addressOrHostname} {machineHostname}!", hostname, machineHostname), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_ThrowsException_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "example.com";
            var port = 1234;

            // Act
            var result = await Format.TryCreateEndpointAsync(hostname, port, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError("Error while trying to resolve hostname: {exMessage} [{hostname}]", It.IsAny<string>(), hostname), Times.Once);
            Assert.Null(result);
        }
    }
}
