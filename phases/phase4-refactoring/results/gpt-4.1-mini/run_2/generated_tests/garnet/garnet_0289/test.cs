using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Xunit;
using Garnet.common;
using Moq;

namespace Garnet.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_EmptyOrWhitespace_ReturnsDefaultBindAny()
        {
            var result = Format.TryCreateEndpoint("", 1234);
            Assert.NotNull(result);
            Assert.Contains(result, ep => ep is IPEndPoint ip && ip.Port == 1234);

            result = Format.TryCreateEndpoint("   ", 1234);
            Assert.NotNull(result);
            Assert.Contains(result, ep => ep is IPEndPoint ip && ip.Port == 1234);
        }

        [Fact]
        public void TryCreateEndpoint_Localhost_ReturnsDefaultBindLoopBack()
        {
            var result = Format.TryCreateEndpoint("localhost", 1234);
            Assert.NotNull(result);
            Assert.Contains(result, ep => ep is IPEndPoint ip && ip.Port == 1234 && (ip.Address.Equals(IPAddress.Loopback) || ip.Address.Equals(IPAddress.IPv6Loopback)));
        }

        [Fact]
        public void TryCreateEndpoint_ValidIPAddress_ReturnsIPEndPoint()
        {
            var ipString = "127.0.0.1";
            var result = Format.TryCreateEndpoint(ipString, 1234);
            Assert.NotNull(result);
            Assert.Single(result);
            var ep = Assert.IsType<IPEndPoint>(result[0]);
            Assert.Equal(IPAddress.Parse(ipString), ep.Address);
            Assert.Equal(1234, ep.Port);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameWithNoAddresses_LogsErrorAndReturnsNull()
        {
            var loggerMock = new Mock<ILogger>();
            var hostname = "no-such-hostname-should-not-exist-1234567890";

            var result = Format.TryCreateEndpoint(hostname, 1234, logger: loggerMock.Object);

            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameDoesNotMatchMachineName_LogsErrorAndReturnsNull()
        {
            var loggerMock = new Mock<ILogger>();
            var fakeHostname = "fakehostname123456";

            var result = Format.TryCreateEndpoint(fakeHostname, 1234, tryConnect: false, logger: loggerMock.Object);

            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("Provided hostname does not much acquired machine name")), It.IsAny<object[]>()), Times.Once);
        }
    }
}
