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
            Assert.Contains(result, ep => ep is IPEndPoint ip && ip.Port == 1234);
        }

        [Fact]
        public void TryCreateEndpoint_WithLocalhost_ReturnsLoopBack()
        {
            var result = Format.TryCreateEndpoint("localhost", 1234);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.True(ep is IPEndPoint));
            Assert.Contains(result, ep => ((IPEndPoint)ep).Address.Equals(IPAddress.Loopback));
        }

        [Fact]
        public void TryCreateEndpoint_WithIpAddress_ReturnsSingleEndpoint()
        {
            var result = Format.TryCreateEndpoint("127.0.0.1", 1234);
            Assert.NotNull(result);
            Assert.Single(result);
            var endpoint = (IPEndPoint)result[0];
            Assert.Equal(IPAddress.Parse("127.0.0.1"), endpoint.Address);
            Assert.Equal(1234, endpoint.Port);
        }

        [Fact]
        public void TryCreateEndpoint_WithHostname_NoIpFound_LogsErrorAndReturnsNull()
        {
            var loggerMock = new Mock<ILogger>();
            var result = Format.TryCreateEndpoint("nonexistenthostname", 1234, logger: loggerMock.Object);
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_WithHostname_TryConnectFalseAndMatchingMachineName_ReturnsEndpoints()
        {
            var hostname = Environment.MachineName;
            var ip = IPAddress.Parse("127.0.0.1");
            // Since Dns.GetHostAddresses is static, we can't mock directly.
            // But we can test with actual hostname, assuming machine name matches.
            var result = Format.TryCreateEndpoint(hostname, 1234);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_WithHostname_TryConnectTrue_ReturnsFirstListeningEndpoint()
        {
            var loggerMock = new Mock<ILogger>();
            // Use localhost which is likely listening
            var result = Format.TryCreateEndpoint("localhost", 80, tryConnect: true, logger: loggerMock.Object);
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public void TryCreateEndpointAsync_WithNullOrEmpty_ReturnsDefaultBindAny()
        {
            var result = Format.TryCreateEndpointAsync(null, 1234).Result;
            Assert.NotNull(result);
            Assert.Contains(result, ep => ep is IPEndPoint ip && ip.Port == 1234);
        }
    }
}
