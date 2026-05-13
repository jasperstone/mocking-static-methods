using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;

namespace Garnet.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_LogsErrorWhenNoIpAddressesFound()
        {
            var loggerMock = new Mock<ILogger>();
            string hostname = "nonexistent.hostname";

            // We simulate no IP addresses found by passing a hostname that resolves to empty array
            // But since the method uses Dns.GetHostAddresses which is static and not mockable,
            // we will test the branch by passing a hostname that is unlikely to resolve.
            // This test will only verify that LogError is called with the expected message.

            var result = Format.TryCreateEndpoint(hostname, 1234, tryConnect: false, loggerMock.Object);

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
        public void TryCreateEndpoint_LogsErrorWhenHostnameDoesNotMatchMachineName()
        {
            var loggerMock = new Mock<ILogger>();
            string fakeHostname = "fakehostname12345";

            // We need to simulate that the hostname resolves to some IP addresses,
            // but the hostname does not match the machine hostname.
            // Since Dns.GetHostAddresses is static and not mockable, we cannot easily simulate this.
            // Instead, we will call TryCreateEndpoint with a hostname that is unlikely to be the machine hostname,
            // and expect the error log.

            var result = Format.TryCreateEndpoint(fakeHostname, 1234, tryConnect: false, loggerMock.Object);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Provided hostname does not much acquired machine name")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_LogsErrorWhenNoReachableIpAddressFound()
        {
            var loggerMock = new Mock<ILogger>();
            string hostname = "localhost";

            // We call TryCreateEndpoint with tryConnect = true and a hostname that resolves to localhost.
            // The method will try to connect to each IP address and if none is reachable, it logs error.
            // Since the TryConnect method uses TcpClient.Connect which is not mockable,
            // and localhost is usually reachable, this test might not hit the error path.
            // So we test with a hostname that resolves but is unreachable by using a fake IP.

            // Instead, we test the private TryConnect method indirectly by passing a hostname that resolves to IPs,
            // but the connection will fail (simulate by passing a hostname that resolves to IPs but no server listening).

            // Unfortunately, without ability to mock TcpClient, this is hard to test fully.
            // So we test the error log by passing a hostname that resolves but no reachable IP.

            // We use a hostname that resolves to IPs but no server listening on port 0 (invalid port).
            var result = Format.TryCreateEndpoint("localhost", 0, tryConnect: true, loggerMock.Object);

            // The result should be null because no reachable IP address found.
            Assert.Null(result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No reachable IP address found for hostname")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_LogsErrorWhenExceptionThrown()
        {
            var loggerMock = new Mock<ILogger>();
            string invalidHostname = "\0invalidhostname";

            // The async method calls Dns.GetHostAddressesAsync which will throw for invalid hostname.
            // We expect the catch block to log an error.

            var result = await Format.TryCreateEndpointAsync(invalidHostname, 1234, tryConnect: false, loggerMock.Object);

            Assert.Null(result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error while trying to resolve hostname")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
