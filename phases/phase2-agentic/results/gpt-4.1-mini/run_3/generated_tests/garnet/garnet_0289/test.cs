using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;

namespace Garnet.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_EmptyOrWhitespace_ReturnsDefaultBindAny()
        {
            var result1 = Format.TryCreateEndpoint("", 1234);
            var result2 = Format.TryCreateEndpoint("   ", 1234);

            Assert.NotNull(result1);
            Assert.NotEmpty(result1);
            Assert.All(result1, ep => Assert.Equal(1234, ((IPEndPoint)ep).Port));

            Assert.NotNull(result2);
            Assert.NotEmpty(result2);
            Assert.All(result2, ep => Assert.Equal(1234, ((IPEndPoint)ep).Port));
        }

        [Fact]
        public void TryCreateEndpoint_Localhost_ReturnsDefaultBindLoopBack()
        {
            var result = Format.TryCreateEndpoint("localhost", 1234);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, ep => Assert.Equal(1234, ((IPEndPoint)ep).Port));
            Assert.Contains(result, ep => ((IPEndPoint)ep).Address.Equals(IPAddress.Loopback));
        }

        [Fact]
        public void TryCreateEndpoint_IPAddress_ReturnsIPEndPoint()
        {
            var ip = "127.0.0.1";
            var port = 1234;
            var result = Format.TryCreateEndpoint(ip, port);

            Assert.NotNull(result);
            Assert.Single(result);
            var ep = Assert.IsType<IPEndPoint>(result[0]);
            Assert.Equal(IPAddress.Parse(ip), ep.Address);
            Assert.Equal(port, ep.Port);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameWithNoIPs_LogsErrorAndReturnsNull()
        {
            var loggerMock = new Mock<ILogger>();
            var hostname = "nonexistent.hostname";

            // We expect LogError to be called with the message about no IP address found
            var result = Format.TryCreateEndpoint(hostname, 1234, logger: loggerMock.Object);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.LogError("No IP address found for hostname:{hostname}", hostname),
                Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameDoesNotMatchMachineName_LogsErrorAndReturnsNull()
        {
            var loggerMock = new Mock<ILogger>();
            var hostname = Dns.GetHostName() + "_diff";

            // We expect LogError to be called with the message about hostname mismatch
            var result = Format.TryCreateEndpoint(hostname, 1234, tryConnect: false, logger: loggerMock.Object);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.LogError(
                    "Provided hostname does not much acquired machine name {addressOrHostname} {machineHostname}!",
                    hostname,
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
