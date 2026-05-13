using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public FormatTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void TryCreateEndpoint_NullOrEmpty_ReturnsDefaultBindAny()
        {
            var result = Format.TryCreateEndpoint(null, 1234, false, _loggerMock.Object);
            Assert.NotNull(result);
            Assert.Contains(result, ep => ep is IPEndPoint);
        }

        [Fact]
        public void TryCreateEndpoint_Localhost_ReturnsLoopback()
        {
            var result = Format.TryCreateEndpoint("localhost", 1234, false, _loggerMock.Object);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.True(ep is IPEndPoint));
        }

        [Fact]
        public void TryCreateEndpoint_IPAddressString_ReturnsIPEndPoint()
        {
            var result = Format.TryCreateEndpoint("127.0.0.1", 1234, false, _loggerMock.Object);
            Assert.NotNull(result);
            Assert.Single(result);
            var endpoint = result.First() as IPEndPoint;
            Assert.NotNull(endpoint);
            Assert.Equal(IPAddress.Parse("127.0.0.1"), endpoint.Address);
            Assert.Equal(1234, endpoint.Port);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameWithNoAddresses_ReturnsNullAndLogsError()
        {
            var hostname = "nonexistenthostname";
            var result = Format.TryCreateEndpoint(hostname, 1234, false, _loggerMock.Object);
            Assert.Null(result);
            _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("No IP address found for hostname")), hostname), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameWithAddressesAndTryConnectFalse_ReturnsEndpointsAndLogsErrorIfUnreachable()
        {
            // Use a hostname that resolves to local IPs
            var hostname = "localhost";
            var result = Format.TryCreateEndpoint(hostname, 80, false, _loggerMock.Object);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
            _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("No reachable IP address")), hostname), Times.Once);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_NullOrWhiteSpace_ReturnsDefaultBindAny()
        {
            var result = await Format.TryCreateEndpointAsync(" ", 1234, false, _loggerMock.Object);
            Assert.NotNull(result);
            Assert.Contains(result, ep => ep is IPEndPoint);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_Localhost_ReturnsLoopback()
        {
            var result = await Format.TryCreateEndpointAsync("localhost", 1234, false, _loggerMock.Object);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.True(ep is IPEndPoint));
        }

        [Fact]
        public async Task TryCreateEndpointAsync_IPAddressString_ReturnsIPEndPoint()
        {
            var result = await Format.TryCreateEndpointAsync("127.0.0.1", 1234, false, _loggerMock.Object);
            Assert.NotNull(result);
            Assert.Single(result);
            var endpoint = result.First() as IPEndPoint;
            Assert.NotNull(endpoint);
            Assert.Equal(IPAddress.Parse("127.0.0.1"), endpoint.Address);
            Assert.Equal(1234, endpoint.Port);
        }
    }
}
