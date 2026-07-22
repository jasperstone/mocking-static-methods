using System;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        private readonly Mock<ILogger> _mockLogger;

        public FormatTests()
        {
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public void TryCreateEndpoint_WhenHostnameDoesNotMatchMachineHostname_LogsErrorMessage()
        {
            // Arrange
            string testHostname = "nonexistenthost.local";
            int port = 8080;

            // Act
            var result = Format.TryCreateEndpoint(testHostname, port, tryConnect: false, _mockLogger.Object);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(formatter => 
                        formatter(null, null) != null &&
                        formatter(null, null).Contains("Provided hostname does not much acquired machine name") &&
                        formatter(null, null).Contains(testHostname))),
                Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_NoIPAddressesFound_LogsError()
        {
            // Arrange
            string invalidHostname = "totally.invalid.hostname12345";
            int port = 8080;

            // Act
            var result = Format.TryCreateEndpoint(invalidHostname, port, tryConnect: false, _mockLogger.Object);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(formatter => 
                        formatter(null, null) != null &&
                        formatter(null, null).Contains("No IP address found for hostname") &&
                        formatter(null, null).Contains(invalidHostname))),
                Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_NoReachableIPAddresses_LogsError()
        {
            // Arrange
            string unreachableHost = "unreachable999.local";
            int port = 9999;

            // Act
            var result = Format.TryCreateEndpoint(unreachableHost, port, tryConnect: true, _mockLogger.Object);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(formatter => 
                        formatter(null, null) != null &&
                        formatter(null, null).Contains("No reachable IP address found for hostname") &&
                        formatter(null, null).Contains(unreachableHost))),
                Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_DnsResolutionException_LogsError()
        {
            // Arrange
            string invalidDns = "¡invalid-dns!@#";

            // Act
            var result = Format.TryCreateEndpoint(invalidDns, 8080, tryConnect: false, _mockLogger.Object);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(formatter => 
                        formatter(null, null) != null &&
                        formatter(null, null).Contains("Error while trying to resolve hostname") &&
                        formatter(null, null).Contains(invalidDns))),
                Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_ValidIP_LogsNothing()
        {
            // Arrange
            string validIP = "127.0.0.1";
            int port = 8080;

            // Act
            var result = Format.TryCreateEndpoint(validIP, port, tryConnect: false, _mockLogger.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            _mockLogger.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), 
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
        }
    }
}
