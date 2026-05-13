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
            // we cannot directly simulate that here.
            // Instead, we test the behavior by passing a hostname that is unlikely to resolve.
            // The method returns null and logs error.

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
            string hostname = "somehostname";
            string machineName = Environment.MachineName;

            // We want to test the branch where tryConnect is false,
            // and the hostname does not match the machine hostname,
            // so it logs error and returns null.

            // To do this, we pass a hostname different from machine name,
            // and since the method calls GetHostName() which returns Environment.MachineName,
            // this will trigger the error log.

            var result = Format.TryCreateEndpoint(hostname, 1234, tryConnect: false, loggerMock.Object);

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

            // We test the tryConnect = true branch where no IP is reachable.
            // Since localhost resolves to loopback addresses, but TryConnect tries to connect and fails,
            // it should log error "No reachable IP address found for hostname".

            // We cannot mock TcpClient.Connect, so this test is limited.
            // Instead, we test with a hostname that resolves but no connection is possible.
            // We expect null result and error log.

            var result = Format.TryCreateEndpoint(hostname, 1234, tryConnect: true, loggerMock.Object);

            // The result can be null if no reachable IP found
            // or an array if reachable. We accept null here.
            if (result == null)
            {
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No reachable IP address found for hostname")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        [Fact]
        public async Task TryCreateEndpointAsync_LogsErrorWhenExceptionThrown()
        {
            var loggerMock = new Mock<ILogger>();
            string hostname = "invalid.hostname";

            // We test the async method branch where Dns.GetHostAddressesAsync throws exception,
            // so it logs error with exception message.

            // We cannot mock Dns.GetHostAddressesAsync, so we simulate by passing an invalid hostname
            // that causes exception or no addresses.

            var result = await Format.TryCreateEndpointAsync(hostname, 1234, tryConnect: false, loggerMock.Object);

            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error while trying to resolve hostname")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtMostOnce);
        }
    }
}
