using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;

namespace Garnet.Tests.Common
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_LogsErrorWhenNoIpAddressFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            string hostname = "nonexistent.hostname.test";
            int port = 1234;

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, tryConnect: false, logger: loggerMock.Object);

            // Assert
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
        public void TryCreateEndpoint_LogsErrorWhenProvidedHostnameDoesNotMatchMachineHostname()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int port = 1234;

            // Use a hostname that resolves to localhost IP but is not the machine hostname
            string fakeHostname = "fakehostname.test";

            // We need to mock Dns.GetHostAddresses and GetHostName to control behavior.
            // Since these are static methods, we cannot mock them directly.
            // Instead, we will test the error logging by calling with a hostname that resolves to IPs but does not match machine hostname.
            // This test depends on the actual machine hostname, so we skip if machine hostname equals fakeHostname.

            string machineHostname = System.Environment.MachineName;
            if (string.Equals(fakeHostname, machineHostname, StringComparison.OrdinalIgnoreCase))
            {
                // Skip test if fakeHostname equals machine hostname
                return;
            }

            // Act
            var result = Format.TryCreateEndpoint(fakeHostname, port, tryConnect: false, logger: loggerMock.Object);

            // Assert
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
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int port = 1234;

            // Use localhost but with tryConnect = true and simulate unreachable IPs by using a hostname that resolves to IPs but no connection possible.
            // We cannot mock TryConnect easily because it's a local function.
            // Instead, we use a hostname that resolves to IPs but no listening service on that port.
            // Using "localhost" with tryConnect true will try to connect to local IPs on port 1234 which likely is closed.

            string hostname = "localhost";

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, tryConnect: true, logger: loggerMock.Object);

            // Assert
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
    }
}
