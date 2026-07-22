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
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_WithLocalhost_ReturnsLoopBack()
        {
            var result = Format.TryCreateEndpoint("localhost", 1234);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
            Assert.Contains(result, ep => ((IPEndPoint)ep).Address.Equals(IPAddress.Loopback));
        }

        [Fact]
        public void TryCreateEndpoint_WithIpAddress_ReturnsSingleEndpoint()
        {
            var ip = "127.0.0.1";
            var result = Format.TryCreateEndpoint(ip, 1234);
            Assert.Single(result);
            var endpoint = (IPEndPoint)result[0];
            Assert.Equal(IPAddress.Parse(ip), endpoint.Address);
            Assert.Equal(1234, endpoint.Port);
        }

        [Fact]
        public void TryCreateEndpoint_WithHostname_NoConnectionLogsError()
        {
            var loggerMock = new Mock<ILogger>();
            var hostname = "localhost"; // assuming local DNS resolves
            var result = Format.TryCreateEndpoint(hostname, 1234, tryConnect: false, logger: loggerMock.Object);
            Assert.NotNull(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void TryCreateEndpoint_WithInvalidHostname_LogsErrorAndReturnsNull()
        {
            var loggerMock = new Mock<ILogger>();
            var result = Format.TryCreateEndpoint("nonexistenthostname", 1234, tryConnect: false, logger: loggerMock.Object);
            Assert.Null(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_WithTryConnect_LogsTrace()
        {
            var loggerMock = new Mock<ILogger>();
            // Use a hostname that resolves to localhost
            var result = Format.TryCreateEndpoint("localhost", 1234, tryConnect: true, logger: loggerMock.Object);
            Assert.NotNull(result);
            loggerMock.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()), Times.AtLeastOnce);
        }
    }
}
