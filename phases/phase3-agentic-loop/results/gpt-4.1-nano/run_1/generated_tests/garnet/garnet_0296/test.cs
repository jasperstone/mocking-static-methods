using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.common;

namespace Garnet.Tests
{
    public class FormatTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public FormatTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void TryCreateEndpoint_WithNullOrWhiteSpace_ReturnsDefaultBindAny()
        {
            var result = Format.TryCreateEndpoint(null, 1234);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_WithIpAddress_ReturnsSingleEndpoint()
        {
            var result = Format.TryCreateEndpoint("127.0.0.1", 1234);
            Assert.Single(result);
            var endpoint = result.First() as IPEndPoint;
            Assert.NotNull(endpoint);
            Assert.Equal(IPAddress.Parse("127.0.0.1"), endpoint.Address);
            Assert.Equal(1234, endpoint.Port);
        }

        [Fact]
        public void TryCreateEndpoint_WithHostnameAndNoConnectivityLogsError()
        {
            // Use a hostname unlikely to resolve
            var hostname = "nonexistenthostname12345";
            var result = Format.TryCreateEndpoint(hostname, 1234, tryConnect: false, logger: _loggerMock.Object);
            Assert.Null(result);
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>(), hostname), Times.Once);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_WithIpAddress_ReturnsSingleEndpoint()
        {
            var result = await Format.TryCreateEndpointAsync("127.0.0.1", 1234);
            Assert.Single(result);
            var endpoint = result.First() as IPEndPoint;
            Assert.NotNull(endpoint);
            Assert.Equal(IPAddress.Parse("127.0.0.1"), endpoint.Address);
            Assert.Equal(1234, endpoint.Port);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_WithHostnameAndNoConnectivityLogsError()
        {
            var hostname = "nonexistenthostname12345";
            var result = await Format.TryCreateEndpointAsync(hostname, 1234, tryConnect: false, logger: _loggerMock.Object);
            Assert.Null(result);
            _loggerMock.Verify(l => l.LogError(It.IsAny<string>(), hostname), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_WithLocalhost_ReturnsLoopbackEndpoints()
        {
            var result = Format.TryCreateEndpoint("localhost", 1234);
            Assert.All(result, ep => Assert.True(((IPEndPoint)ep).Address.IsLoopback));
        }

        [Fact]
        public void TryCreateEndpoint_WithNegativePrefix_ReturnsDefaultBindAny()
        {
            var result = Format.TryCreateEndpoint("-localhost", 1234);
            Assert.NotNull(result);
        }
    }
}
