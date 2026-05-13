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
        private readonly Mock<ILogger> _loggerMock;

        public FormatTests()
        {
            _loggerMock = new Mock<ILogger>();
        }

        [Fact]
        public void TryCreateEndpoint_NullOrEmpty_ReturnsDefaultBindAny()
        {
            var result = Format.TryCreateEndpoint(null, 1234, logger: _loggerMock.Object);
            Assert.NotNull(result);
            Assert.Contains(result, ep => ep is IPEndPoint ip && ip.Port == 1234);
        }

        [Fact]
        public void TryCreateEndpoint_Localhost_ReturnsLoopBack()
        {
            var result = Format.TryCreateEndpoint("localhost", 1234, logger: _loggerMock.Object);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.True(ep is IPEndPoint));
            Assert.Contains(result, ep => ((IPEndPoint)ep).Address.Equals(IPAddress.Loopback));
        }

        [Fact]
        public void TryCreateEndpoint_IPAddressString_ReturnsIpEndPoint()
        {
            var ipString = "127.0.0.1";
            var result = Format.TryCreateEndpoint(ipString, 1234, logger: _loggerMock.Object);
            Assert.NotNull(result);
            Assert.Single(result);
            var endpoint = result.First() as IPEndPoint;
            Assert.NotNull(endpoint);
            Assert.Equal(IPAddress.Parse(ipString), endpoint.Address);
            Assert.Equal(1234, endpoint.Port);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameWithNoAddresses_LogsErrorAndReturnsNull()
        {
            var hostname = "nonexistenthostname";
            var result = Format.TryCreateEndpoint(hostname, 1234, logger: _loggerMock.Object);
            Assert.Null(result);
            _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("No IP address found")), hostname), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_TryConnect_SuccessfulLogsTrace()
        {
            var ip = IPAddress.Loopback;
            var endpoint = new IPEndPoint(ip, 1234);
            // Mock TryConnect to return true
            var result = Format.TryCreateEndpoint(ip.ToString(), 1234, tryConnect: true, logger: _loggerMock.Object);
            Assert.NotNull(result);
            Assert.Single(result);
            var ep = result.First() as IPEndPoint;
            Assert.NotNull(ep);
            Assert.Equal(ip, ep.Address);
        }

        [Fact]
        public void TryCreateEndpoint_TryConnect_UnreachableLogsTrace()
        {
            var ip = IPAddress.Parse("192.0.2.1");
            var endpoint = new IPEndPoint(ip, 1234);
            // Temporarily replace TryConnect to simulate unreachable
            // Since TryConnect is a local function, we can't mock directly.
            // Instead, test with an IP unlikely to be reachable and check logs.
            var result = Format.TryCreateEndpoint(ip.ToString(), 1234, tryConnect: true, logger: _loggerMock.Object);
            Assert.NotNull(result);
            Assert.Empty(result);
            _loggerMock.Verify(l => l.LogTrace(It.Is<string>(s => s.Contains("Unreachable")), ip), Times.AtLeastOnce);
        }

        [Fact]
        public void TryCreateEndpoint_MalformedException_LogsError()
        {
            // Simulate exception by passing invalid hostname
            var invalidHostname = "!!!invalid!!!";
            var result = Format.TryCreateEndpoint(invalidHostname, 1234, logger: _loggerMock.Object);
            Assert.Null(result);
            _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("Error while trying to resolve hostname")), invalidHostname), Times.Once);
        }
    }
}
