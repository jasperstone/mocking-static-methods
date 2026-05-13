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
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_Localhost_ReturnsLoopBack()
        {
            var result = Format.TryCreateEndpoint("localhost", 1234, logger: _loggerMock.Object);
            Assert.Contains(result, ep => ((IPEndPoint)ep).Address.Equals(IPAddress.Loopback));
        }

        [Fact]
        public void TryCreateEndpoint_IPAddressString_ReturnsSingleEndpoint()
        {
            var ipString = "127.0.0.1";
            var result = Format.TryCreateEndpoint(ipString, 1234, logger: _loggerMock.Object);
            Assert.Single(result);
            var endpoint = (IPEndPoint)result[0];
            Assert.Equal(IPAddress.Parse(ipString), endpoint.Address);
            Assert.Equal(1234, endpoint.Port);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameWithNoAddresses_ReturnsNullAndLogsError()
        {
            var hostname = "nonexistent.hostname";
            var result = Format.TryCreateEndpoint(hostname, 1234, logger: _loggerMock.Object);
            Assert.Null(result);
            _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("No IP address found")), hostname), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameWithAddressesAndTryConnectFalse_ReturnsEndpointsAndLogsErrorIfNotMatchingMachine()
        {
            var hostname = "localhost"; // assuming machine hostname is different
            var result = Format.TryCreateEndpoint(hostname, 1234, tryConnect: false, logger: _loggerMock.Object);
            Assert.NotNull(result);
            _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("does not much")), hostname), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameWithAddressesAndTryConnectTrue_ReturnsEndpointIfListening()
        {
            var hostname = "localhost"; // assuming machine hostname is different
            // Mock TryConnect to return true for the first endpoint
            var originalMethod = typeof(Format).GetMethod("TryCreateEndpoint", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            // Since we can't override static methods directly, this test assumes local environment
            // and that localhost resolves to 127.0.0.1 which is listening.
            var result = Format.TryCreateEndpoint(hostname, 1234, tryConnect: true, logger: _loggerMock.Object);
            Assert.NotNull(result);
        }
    }
}
