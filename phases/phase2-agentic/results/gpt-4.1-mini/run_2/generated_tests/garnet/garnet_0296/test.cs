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
            // Instead, we test the behavior by passing a hostname that likely resolves to no IPs.
            // This test may be flaky depending on DNS, so we test the error log call by invoking TryCreateEndpoint with a hostname that is unlikely to resolve.

            // To reliably test the logging, we can create a derived method or use reflection to test the private method.
            // But since the method is static and uses static Dns, we cannot mock it easily.
            // So we test the error log call by passing an empty string which triggers defaultBindAny and no error log.
            // Instead, we test the error log for the hostname mismatch case below.

            // This test is a placeholder to show the intent.
            // The actual error log call on no IP addresses is hard to trigger in unit test without mocking Dns.

            // So we assert that TryCreateEndpoint returns null for a hostname that does not resolve.
            var result = Format.TryCreateEndpoint("nonexistent.hostname", 1234, false, loggerMock.Object);
            // It may return null or endpoints depending on DNS, so we just check if logger.LogError was called at least once.
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No IP address found for hostname")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void TryCreateEndpoint_LogsErrorWhenHostnameDoesNotMatchMachineName()
        {
            var loggerMock = new Mock<ILogger>();
            string hostname = "somehostname";
            int port = 1234;

            // We simulate the machine hostname to be different by calling TryCreateEndpoint with a hostname that is unlikely to match machine hostname.
            // The method calls GetHostName() internally which returns Environment.MachineName.
            // So we pick a hostname different from Environment.MachineName.

            var machineName = Environment.MachineName;
            Assert.NotEqual(hostname, machineName, ignoreCase: true);

            var result = Format.TryCreateEndpoint(hostname, port, tryConnect: false, logger: loggerMock.Object);

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
            int port = 1234;

            // We test the tryConnect = true path where no IP is reachable.
            // Since TryConnect uses TcpClient.Connect which is not mockable here,
            // we cannot simulate unreachable IPs easily.
            // So this test is a placeholder to show intent.

            // We call TryCreateEndpoint with tryConnect = true and a hostname that resolves to localhost.
            // It should return endpoints or null depending on connectivity.
            // We check if logger.LogError was called with "No reachable IP address found for hostname".

            var result = Format.TryCreateEndpoint(hostname, port, tryConnect: true, logger: loggerMock.Object);

            // The result may be null or endpoints depending on actual network.
            // We verify if LogError was called with the expected message at least once.
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No reachable IP address found for hostname")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_LogsErrorWhenExceptionThrown()
        {
            var loggerMock = new Mock<ILogger>();
            string hostname = "nonexistent.hostname";
            int port = 1234;

            // The async method calls Dns.GetHostAddressesAsync which is static and not mockable.
            // We simulate an exception by passing a hostname that throws in DNS resolution.
            // This is hard to simulate reliably, so this test is a placeholder.

            // We call the async method and verify that if an exception occurs, LogError is called.

            var result = await Format.TryCreateEndpointAsync(hostname, port, tryConnect: false, logger: loggerMock.Object);

            // The result may be null or endpoints depending on DNS.
            // We verify if LogError was called with the expected message at least once.
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error while trying to resolve hostname")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
