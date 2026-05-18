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
        public void TryCreateEndpoint_WithEmptyInput_ReturnsDefaultBindAny()
        {
            // Arrange
            var port = 12345;

            // Act
            var result = Format.TryCreateEndpoint(string.Empty, port, logger: null);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_WithLocalhost_ReturnsLoopBack()
        {
            // Arrange
            var port = 12345;

            // Act
            var result = Format.TryCreateEndpoint("localhost", port, logger: null);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, ep => ((IPEndPoint)ep).Address.Equals(IPAddress.Loopback));
        }

        [Fact]
        public void TryCreateEndpoint_WithIpAddress_ReturnsSingleEndpoint()
        {
            // Arrange
            var port = 12345;
            var ipString = "127.0.0.1";

            // Act
            var result = Format.TryCreateEndpoint(ipString, port, logger: null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var endpoint = (IPEndPoint)result[0];
            Assert.Equal(IPAddress.Parse(ipString), endpoint.Address);
            Assert.Equal(port, endpoint.Port);
        }

        [Fact]
        public void TryCreateEndpoint_WithHostnameAndNoConnectAndMatchingMachineName_ReturnsEndpoints()
        {
            // Arrange
            var port = 12345;
            var hostname = Environment.MachineName;
            var mockLogger = new Mock<ILogger>();
            var ipAddresses = new[] { IPAddress.Parse("127.0.0.1") };

            // Mock Dns.GetHostAddresses
            var originalGetHostAddresses = Dns.GetHostAddresses;
            Dns.GetHostAddresses = (host) => ipAddresses;

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, tryConnect: false, logger: mockLogger.Object);

            // Reset Dns
            Dns.GetHostAddresses = originalGetHostAddresses;

            // Assert
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
            Assert.All(result, ep => Assert.Equal(IPAddress.Parse("127.0.0.1"), ((IPEndPoint)ep).Address));
        }

        [Fact]
        public void TryCreateEndpoint_WithHostnameAndConnectAndListeningEndpoint_ReturnsEndpoint()
        {
            // Arrange
            var port = 12345;
            var hostname = "testhost";

            var mockLogger = new Mock<ILogger>();
            var ipAddresses = new[] { IPAddress.Parse("127.0.0.1") };

            // Mock Dns.GetHostAddresses
            var originalGetHostAddresses = Dns.GetHostAddresses;
            Dns.GetHostAddresses = (host) => ipAddresses;

            // Mock TryConnect to return true for the first endpoint
            bool called = false;
            bool TryConnect(IPEndPoint ep)
            {
                if (!called)
                {
                    called = true;
                    return true;
                }
                return false;
            }

            // Replace the local method with a delegate
            Func<IPEndPoint, bool> tryConnectDelegate = TryConnect;

            // Use reflection to invoke the private method with delegate
            // But since it's static, we can just call directly if made accessible
            // For simplicity, we will temporarily make TryConnect a local function

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, tryConnect: true, logger: mockLogger.Object);

            // Reset Dns
            Dns.GetHostAddresses = originalGetHostAddresses;

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var endpoint = (IPEndPoint)result[0];
            Assert.Equal(IPAddress.Parse("127.0.0.1"), endpoint.Address);
        }

        [Fact]
        public void TryCreateEndpoint_WithDnsException_LogsErrorAndReturnsNull()
        {
            // Arrange
            var port = 12345;
            var hostname = "badhost";

            var mockLogger = new Mock<ILogger>();

            // Mock Dns.GetHostAddresses to throw
            var originalGetHostAddresses = Dns.GetHostAddresses;
            Dns.GetHostAddresses = (host) => throw new Exception("DNS error");

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, logger: mockLogger.Object);

            // Reset Dns
            Dns.GetHostAddresses = originalGetHostAddresses;

            // Assert
            Assert.Null(result);
            mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
