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
        public void TryCreateEndpoint_LogsErrorWhenNoIpAddressesFound()
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
        public void TryCreateEndpoint_LogsErrorWhenProvidedHostnameDoesNotMatchMachineName()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int port = 1234;
            string fakeHostname = "fakehostname1234";

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

            // Use localhost which resolves to IPs, but tryConnect = true and no ports open on localhost:1234 (likely)
            string hostname = "localhost";

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, tryConnect: true, logger: loggerMock.Object);

            // Assert
            // It should log error about no reachable IP address found
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
