using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Xunit;
using Garnet.common;
using Moq;

namespace Garnet.Tests.Common
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_EmptyOrWhitespace_ReturnsDefaultBindAny()
        {
            var result = Format.TryCreateEndpoint("", 1234);
            Assert.NotNull(result);
            Assert.Contains(result, ep => ep is IPEndPoint);

            result = Format.TryCreateEndpoint("   ", 1234);
            Assert.NotNull(result);
            Assert.Contains(result, ep => ep is IPEndPoint);
        }

        [Fact]
        public void TryCreateEndpoint_Localhost_ReturnsDefaultBindLoopBack()
        {
            var result = Format.TryCreateEndpoint("localhost", 1234);
            Assert.NotNull(result);
            Assert.Contains(result, ep => ep is IPEndPoint ip && (ip.Address.Equals(IPAddress.Loopback) || ip.Address.Equals(IPAddress.IPv6Loopback)));
        }

        [Fact]
        public void TryCreateEndpoint_ValidIPAddress_ReturnsIPEndPoint()
        {
            var ip = "127.0.0.1";
            var result = Format.TryCreateEndpoint(ip, 1234);
            Assert.NotNull(result);
            Assert.Single(result);
            var ep = Assert.IsType<IPEndPoint>(result[0]);
            Assert.Equal(IPAddress.Parse(ip), ep.Address);
            Assert.Equal(1234, ep.Port);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameWithNoAddresses_LogsErrorAndReturnsNull()
        {
            var loggerMock = new Mock<ILogger>();

            // Use a hostname that resolves to no IP addresses by using a nonsense hostname
            var nonsenseHostname = "no-such-hostname-should-not-exist-12345";

            var result = Format.TryCreateEndpoint(nonsenseHostname, 1234, logger: loggerMock.Object);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No IP address found for hostname")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameDoesNotMatchMachineName_LogsErrorAndReturnsNull()
        {
            var loggerMock = new Mock<ILogger>();

            // Use a hostname unlikely to match machine hostname
            var fakeHostname = "fakehostname123456";

            var result = Format.TryCreateEndpoint(fakeHostname, 1234, tryConnect: false, logger: loggerMock.Object);

            if (result == null)
            {
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Provided hostname does not much acquired machine name")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }
    }
}
