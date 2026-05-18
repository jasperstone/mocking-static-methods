using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Garnet.common;

namespace Garnet.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_EmptyOrWhitespace_ReturnsDefaultBindAny()
        {
            var logger = new Mock<ILogger>();
            var result1 = Format.TryCreateEndpoint("", 1234, logger: logger.Object);
            var result2 = Format.TryCreateEndpoint("   ", 1234, logger: logger.Object);

            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.Contains(result1, ep => ep is IPEndPoint);
            Assert.Contains(result2, ep => ep is IPEndPoint);
        }

        [Fact]
        public void TryCreateEndpoint_Localhost_ReturnsDefaultBindLoopBack()
        {
            var logger = new Mock<ILogger>();
            var result = Format.TryCreateEndpoint("localhost", 1234, logger: logger.Object);

            Assert.NotNull(result);
            Assert.Contains(result, ep => ep is IPEndPoint ip && (ip.Address.Equals(IPAddress.Loopback) || ip.Address.Equals(IPAddress.IPv6Loopback)));
        }

        [Fact]
        public void TryCreateEndpoint_IPAddress_ReturnsIPEndPoint()
        {
            var logger = new Mock<ILogger>();
            var ipString = "127.0.0.1";
            var result = Format.TryCreateEndpoint(ipString, 1234, logger: logger.Object);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.IsType<IPEndPoint>(result[0]);
            var ep = (IPEndPoint)result[0];
            Assert.Equal(IPAddress.Parse(ipString), ep.Address);
            Assert.Equal(1234, ep.Port);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameWithNoAddresses_LogsErrorAndReturnsNull()
        {
            var logger = new Mock<ILogger>();

            // Use a hostname that resolves to no IP addresses by using a nonsense hostname
            var hostname = "no-such-hostname-xyz-1234";

            var result = Format.TryCreateEndpoint(hostname, 1234, logger: logger.Object);

            Assert.Null(result);
            logger.Verify(l => l.LogError("No IP address found for hostname:{hostname}", hostname), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameDoesNotMatchMachineName_LogsErrorAndReturnsNull()
        {
            var logger = new Mock<ILogger>();

            // Use a hostname that resolves to IP addresses but is unlikely to match machine hostname
            var hostname = "example.com";
            var port = 1234;

            var result = Format.TryCreateEndpoint(hostname, port, tryConnect: false, logger: logger.Object);

            Assert.Null(result);
            logger.Verify(l => l.LogError(It.Is<string>(s => s.StartsWith("Provided hostname does not much acquired machine name")), hostname, It.IsAny<string>()), Times.Once);
        }
    }
}
