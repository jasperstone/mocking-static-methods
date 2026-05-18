using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_NoIpAddressesFound_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            string invalidHostname = "nonexistenthost12345";

            // Act
            var result = Format.TryCreateEndpoint(invalidHostname, 6379, logger: mockLogger.Object);

            // Assert
            Assert.Null(result);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state?.ToString()?.Contains("No IP address found for hostname:" + invalidHostname) == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameMismatch_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            string fakeHostname = "fakehost.local";

            // Act
            var result = Format.TryCreateEndpoint(fakeHostname, 6379, tryConnect: false, logger: mockLogger.Object);

            // Assert
            Assert.Null(result);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state?.ToString()?.Contains("Provided hostname does not much acquired machine name") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryCreateEndpoint_DnsException_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            string invalidDnsInput = new string('a', 300);

            // Act
            var result = Format.TryCreateEndpoint(invalidDnsInput, 6379, logger: mockLogger.Object);

            // Assert
            Assert.Null(result);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state?.ToString()?.Contains("Error while trying to resolve hostname") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void TryCreateEndpoint_NoReachableEndpointWithTryConnect_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            string hostname = "localhost";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 99999, tryConnect: true, logger: mockLogger.Object);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => state?.ToString()?.Contains("No reachable IP address found for hostname:localhost") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
