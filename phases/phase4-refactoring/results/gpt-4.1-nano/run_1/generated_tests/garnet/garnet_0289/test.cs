using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.common.Tests
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
        public void TryCreateEndpoint_WithLoopbackHostname_ReturnsLoopbackEndpoints()
        {
            var result = Format.TryCreateEndpoint("localhost", 1234);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.Contains("127.0.0.1", ep.ToString()));
        }

        [Fact]
        public void TryCreateEndpoint_WithIpAddress_ReturnsSingleEndpoint()
        {
            var result = Format.TryCreateEndpoint("127.0.0.1", 1234);
            Assert.Single(result);
            Assert.Equal(IPAddress.Parse("127.0.0.1"), ((IPEndPoint)result[0]).Address);
        }

        [Fact]
        public void TryCreateEndpoint_WithInvalidHostname_LogsErrorAndReturnsNull()
        {
            var loggerMock = new Mock<ILogger>();
            var result = Format.TryCreateEndpoint("invalidhostname", 1234, logger: loggerMock.Object);
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_WithTryConnectAndListening_ReturnsEndpoint()
        {
            var loggerMock = new Mock<ILogger>();
            // Use localhost which is reachable
            var result = Format.TryCreateEndpoint("localhost", 80, tryConnect: true, logger: loggerMock.Object);
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async void TryCreateEndpointAsync_WithNullOrEmpty_ReturnsDefaultBindAny()
        {
            var result = await Format.TryCreateEndpointAsync(null, 1234);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public async void TryCreateEndpointAsync_WithIpAddress_ReturnsSingleEndpoint()
        {
            var result = await Format.TryCreateEndpointAsync("127.0.0.1", 1234);
            Assert.Single(result);
            Assert.Equal(IPAddress.Parse("127.0.0.1"), ((IPEndPoint)result[0]).Address);
        }
    }
}
