using System;
using System.Linq;
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
        public void TryCreateEndpoint_ReturnsLoopback_ForLocalhost()
        {
            var result = Format.TryCreateEndpoint("localhost", 1234);
            Assert.NotNull(result);
            Assert.Contains(result, ep => ((IPEndPoint)ep).Address.Equals(IPAddress.Loopback));
        }

        [Fact]
        public void TryCreateEndpoint_ReturnsAny_ForEmptyString()
        {
            var result = Format.TryCreateEndpoint("", 1234);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_ReturnsNull_AndLogsError_ForUnknownHostname()
        {
            var loggerMock = new Mock<ILogger>();
            var result = Format.TryCreateEndpoint("unknownhostname", 1234, logger: loggerMock.Object);
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_ReturnsEndpoint_ForIpAddress()
        {
            var ip = IPAddress.Parse("127.0.0.1");
            var result = Format.TryCreateEndpoint(ip.ToString(), 1234);
            Assert.NotNull(result);
            var endpoint = result.First() as IPEndPoint;
            Assert.NotNull(endpoint);
            Assert.Equal(ip, endpoint.Address);
            Assert.Equal(1234, endpoint.Port);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_ReturnsLoopback_ForLocalhost()
        {
            var result = await Format.TryCreateEndpointAsync("localhost", 1234);
            Assert.NotNull(result);
            Assert.Contains(result, ep => ((IPEndPoint)ep).Address.Equals(IPAddress.Loopback));
        }

        [Fact]
        public async Task TryCreateEndpointAsync_ReturnsAny_ForEmptyString()
        {
            var result = await Format.TryCreateEndpointAsync("", 1234);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public async Task TryCreateEndpointAsync_ReturnsNull_AndLogsError_ForUnknownHostname()
        {
            var loggerMock = new Mock<ILogger>();
            var result = await Format.TryCreateEndpointAsync("unknownhostname", 1234, logger: loggerMock.Object);
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_ReturnsEndpoint_ForIpAddress()
        {
            var ip = IPAddress.Parse("127.0.0.1");
            var result = await Format.TryCreateEndpointAsync(ip.ToString(), 1234);
            Assert.NotNull(result);
            var endpoint = result.First() as IPEndPoint;
            Assert.NotNull(endpoint);
            Assert.Equal(ip, endpoint.Address);
            Assert.Equal(1234, endpoint.Port);
        }
    }
}
