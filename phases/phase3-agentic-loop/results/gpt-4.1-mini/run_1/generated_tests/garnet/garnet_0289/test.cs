using System;
using System.Net;
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
            string hostname = "somehostname";
            int port = 1234;

            // Act
            var result = Format.TryCreateEndpoint(hostname, port, tryConnect: false, logger: loggerMock.Object);

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
            string hostname = "localhost";
            int port = 1234;

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
