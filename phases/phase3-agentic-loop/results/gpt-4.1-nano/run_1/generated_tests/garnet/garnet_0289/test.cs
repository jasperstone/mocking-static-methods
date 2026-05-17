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
            var result = Format.TryCreateEndpoint(null, port);

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
            var result = Format.TryCreateEndpoint(string.Empty, port);

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
            var result = Format.TryCreateEndpoint("localhost", port);

            // Assert
            Assert.Contains(result, ep => ((IPEndPoint)ep).Address.Equals(IPAddress.Loopback));
        }

        [Fact]
        public void TryCreateEndpoint_WithIpAddress_ReturnsSingleEndpoint()
        {
            // Arrange
            var port = 12345;
            var ipString = "127.0.0.1";

            // Act
            var result = Format.TryCreateEndpoint(ipString, port);

            // Assert
            Assert.Single(result);
            var endpoint = (IPEndPoint)result[0];
            Assert.Equal(IPAddress.Parse(ipString), endpoint.Address);
            Assert.Equal(port, endpoint.Port);
        }

        [Fact]
        public void TryCreateEndpoint_WithHostnameAndNoConnect_LogsErrorAndReturnsNull()
        {
            // Arrange
            var port = 12345;
            var hostname = "nonexistenthostname";
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()))
                      .Verifiable();

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, tryConnect: false, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(x => x.LogError(It.Is<string>(msg => msg.Contains("Provided hostname does not much")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_WithHostnameAndConnect_ReturnsEndpointIfListening()
        {
            // Arrange
            var port = 12345;
            var hostname = "localhost";

            // Mock Dns.GetHostAddresses to return local IPs
            var originalGetHostAddresses = Dns.GetHostAddresses;
            Dns.GetHostAddresses = (host) => new[] { IPAddress.Loopback };

            // Mock TryConnect to always return true
            bool TryConnect(IPEndPoint ep) => true;

            // Use reflection to replace the method (or alternatively, modify the code to allow injection)
            // For simplicity, assume TryConnect is static and can be replaced (not trivial in real code)
            // Here, we just test the code assuming TryConnect always returns true

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, tryConnect: true);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var endpoint = (IPEndPoint)result[0];
            Assert.Equal(IPAddress.Loopback, endpoint.Address);
            Assert.Equal(port, endpoint.Port);

            // Cleanup
            Dns.GetHostAddresses = originalGetHostAddresses;
        }

        [Fact]
        public void TryCreateEndpoint_WithExceptionLogsError()
        {
            // Arrange
            var port = 12345;
            var hostname = "errorhost";

            // Mock Dns.GetHostAddresses to throw
            var originalGetHostAddresses = Dns.GetHostAddresses;
            Dns.GetHostAddresses = (host) => throw new Exception("DNS error");

            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable();

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, logger: loggerMock.Object);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(x => x.LogError(It.Is<string>(msg => msg.Contains("Error while trying to resolve hostname")), It.IsAny<object[]>()), Times.Once);

            // Cleanup
            Dns.GetHostAddresses = originalGetHostAddresses;
        }
    }
}
