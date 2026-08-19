using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Garnet.common;
using System;

namespace Garnet.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_WithNullOrEmpty_ReturnsDefaultBindAny()
        {
            var result = Format.TryCreateEndpoint(null, 1234);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_WithLocalhost_ReturnsLoopback()
        {
            var result = Format.TryCreateEndpoint("localhost", 1234);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
            Assert.Contains(result, ep => ((IPEndPoint)ep).Address.Equals(IPAddress.Loopback));
        }

        [Fact]
        public void TryCreateEndpoint_WithIPAddress_ReturnsSingleEndpoint()
        {
            var result = Format.TryCreateEndpoint("127.0.0.1", 1234);
            Assert.Single(result);
            var endpoint = (IPEndPoint)result[0];
            Assert.Equal(IPAddress.Parse("127.0.0.1"), endpoint.Address);
            Assert.Equal(1234, endpoint.Port);
        }

        [Fact]
        public void TryCreateEndpoint_WithHostname_NoConnectionLogsError()
        {
            var loggerMock = new Mock<ILogger>();
            var result = Format.TryCreateEndpoint("nonexistenthostname", 1234, tryConnect: false, logger: loggerMock.Object);
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_WithNullOrEmpty_ReturnsDefaultBindAny()
        {
            var result = await Format.TryCreateEndpointAsync(null, 1234);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public async Task TryCreateEndpointAsync_WithIPAddress_ReturnsSingleEndpoint()
        {
            var result = await Format.TryCreateEndpointAsync("127.0.0.1", 1234);
            Assert.Single(result);
            var endpoint = (IPEndPoint)result[0];
            Assert.Equal(IPAddress.Parse("127.0.0.1"), endpoint.Address);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_WithHostnameLogsErrorOnFailure()
        {
            var loggerMock = new Mock<ILogger>();
            var result = await Format.TryCreateEndpointAsync("nonexistenthostname", 1234, logger: loggerMock.Object);
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
