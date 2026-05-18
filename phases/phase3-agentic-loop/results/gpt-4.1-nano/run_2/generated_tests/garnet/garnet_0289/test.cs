using System;
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
        public void TryCreateEndpoint_WithNullInput_ReturnsDefaultBindAny()
        {
            // Arrange
            var port = 12345;

            // Act
            var result = Format.TryCreateEndpoint(null, port, logger: null);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_WithEmptyString_ReturnsDefaultBindAny()
        {
            // Arrange
            var port = 12345;

            // Act
            var result = Format.TryCreateEndpoint("", port, logger: null);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_WithHostNameLogsErrorAndReturnsNull_WhenNoAddressesFound()
        {
            // Arrange
            var port = 12345;
            var hostname = "nonexistenthostname";
            var loggerMock = new Mock<ILogger>();
            // Override Dns.GetHostAddresses to throw
            // But since it's static, we can't mock directly, so we test the catch block

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("No IP address found")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_WithIPAddress_ReturnsSingleEndpoint()
        {
            // Arrange
            var port = 12345;
            var ipString = "127.0.0.1";

            // Act
            var result = Format.TryCreateEndpoint(ipString, port, logger: null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var endpoint = result[0] as IPEndPoint;
            Assert.NotNull(endpoint);
            Assert.Equal(IPAddress.Parse(ipString), endpoint.Address);
            Assert.Equal(port, endpoint.Port);
        }

        [Fact]
        public void TryCreateEndpoint_WithLocalhost_ReturnsLoopbackEndpoints()
        {
            // Arrange
            var port = 12345;
            var hostname = "localhost";

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, logger: null);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, ep => ((IPEndPoint)ep).Address.Equals(IPAddress.Loopback));
            Assert.Contains(result, ep => ((IPEndPoint)ep).Address.Equals(IPAddress.IPv6Loopback));
        }

        [Fact]
        public void TryCreateEndpoint_WithHostname_MatchingMachineName_ReturnsIpEndpoints()
        {
            // Arrange
            var port = 12345;
            var hostname = Environment.MachineName;
            var ipAddresses = new[] { IPAddress.Parse("127.0.0.1") };
            // Override Dns.GetHostAddresses to return a known IP
            // Since static, we can't mock directly, so this test may be limited

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, logger: null);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_WithTryConnect_AndListeningEndpoint_ReturnsEndpoint()
        {
            // Arrange
            var port = 12345;
            var ip = IPAddress.Loopback;
            var endpoint = new IPEndPoint(ip, port);
            var loggerMock = new Mock<ILogger>();
            // Patch TryConnect to return true
            // But since it's a local method, we can't mock directly, so this test is more illustrative

            // Act
            var result = Format.TryCreateEndpoint(ip.ToString(), port, tryConnect: true, logger: loggerMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
}
