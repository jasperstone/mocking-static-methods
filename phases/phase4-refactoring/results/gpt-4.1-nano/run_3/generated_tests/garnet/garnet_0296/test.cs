using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
            var port = 12345;
            var result = Format.TryCreateEndpoint(null, port);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_WithEmptyInput_ReturnsDefaultBindAny()
        {
            var port = 12345;
            var result = Format.TryCreateEndpoint("", port);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public void TryCreateEndpoint_WithLocalhost_ReturnsLoopback()
        {
            var port = 12345;
            var result = Format.TryCreateEndpoint("localhost", port);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
            Assert.All(result, ep => Assert.True(((IPEndPoint)ep).Address.IsLoopback));
        }

        [Fact]
        public void TryCreateEndpoint_WithIpAddress_ReturnsSingleEndpoint()
        {
            var port = 12345;
            var ip = "127.0.0.1";
            var result = Format.TryCreateEndpoint(ip, port);
            Assert.Single(result);
            var endpoint = result.First() as IPEndPoint;
            Assert.NotNull(endpoint);
            Assert.Equal(IPAddress.Parse(ip), endpoint.Address);
            Assert.Equal(port, endpoint.Port);
        }

        [Fact]
        public void TryCreateEndpoint_WithInvalidHostname_ReturnsNullAndLogsError()
        {
            var port = 12345;
            var hostname = "nonexistenthostname12345";
            var loggerMock = new Mock<ILogger>();
            var result = Format.TryCreateEndpoint(hostname, port, logger: loggerMock.Object);
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
        public async Task TryCreateEndpointAsync_WithNullInput_ReturnsDefaultBindAny()
        {
            var port = 12345;
            var result = await Format.TryCreateEndpointAsync(null, port);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
        }

        [Fact]
        public async Task TryCreateEndpointAsync_WithLocalhost_ReturnsLoopback()
        {
            var port = 12345;
            var result = await Format.TryCreateEndpointAsync("localhost", port);
            Assert.NotNull(result);
            Assert.All(result, ep => Assert.IsType<IPEndPoint>(ep));
            Assert.All(result, ep => Assert.True(((IPEndPoint)ep).Address.IsLoopback));
        }

        [Fact]
        public async Task TryCreateEndpointAsync_WithIpAddress_ReturnsSingleEndpoint()
        {
            var port = 12345;
            var ip = "127.0.0.1";
            var result = await Format.TryCreateEndpointAsync(ip, port);
            Assert.Single(result);
            var endpoint = result.First() as IPEndPoint;
            Assert.NotNull(endpoint);
            Assert.Equal(IPAddress.Parse(ip), endpoint.Address);
            Assert.Equal(port, endpoint.Port);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_WithInvalidHostname_ReturnsNullAndLogsError()
        {
            var port = 12345;
            var hostname = "nonexistenthostname12345";
            var loggerMock = new Mock<ILogger>();
            var result = await Format.TryCreateEndpointAsync(hostname, port, logger: loggerMock.Object);
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
    }
}
