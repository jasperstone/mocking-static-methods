using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        [Fact]
        public async Task TryCreateEndpointAsync_NoIPAddressesFound_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var addresses = new IPAddress[0];

            var dnsMock = new Mock<IDnsResolver>();
            dnsMock.Setup(d => d.GetHostAddressesAsync(It.IsAny<string>())).ReturnsAsync(addresses);

            // Act
            var result = await Format.TryCreateEndpointAsync("nonexistent.hostname", 80, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_HostnameMismatch_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var addresses = new[] { IPAddress.Loopback };

            var dnsMock = new Mock<IDnsResolver>();
            dnsMock.Setup(d => d.GetHostAddressesAsync(It.IsAny<string>())).ReturnsAsync(addresses);

            var machineHostname = "machine.local";
            var formatMock = new Mock<IFormat>();
            formatMock.Setup(f => f.GetHostName()).Returns(machineHostname);

            // Act
            var result = await Format.TryCreateEndpointAsync("different.hostname", 80, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_NoReachableIP_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var addresses = new[] { IPAddress.Loopback };

            var dnsMock = new Mock<IDnsResolver>();
            dnsMock.Setup(d => d.GetHostAddressesAsync(It.IsAny<string>())).ReturnsAsync(addresses);

            var tcpClientMock = new Mock<ITcpClient>();
            tcpClientMock.Setup(tc => tc.TryConnectAsync(It.IsAny<IPEndPoint>(), It.IsAny<ILogger>())).ReturnsAsync(false);

            // Act
            var result = await Format.TryCreateEndpointAsync("localhost", 80, true, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_ExceptionDuringResolution_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            var dnsMock = new Mock<IDnsResolver>();
            dnsMock.Setup(d => d.GetHostAddressesAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Resolution error"));

            // Act
            var result = await Format.TryCreateEndpointAsync("error.hostname", 80, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.Null(result);
        }
    }

    // Mock interfaces for testing
    public interface IDnsResolver
    {
        Task<IPAddress[]> GetHostAddressesAsync(string hostname);
    }

    public interface ITcpClient
    {
        Task<bool> TryConnectAsync(IPEndPoint endpoint, ILogger logger);
    }

    public interface IFormat
    {
        string GetHostName();
    }
}
