using System;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_LogsError_WhenNoIpAddressesFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            string hostname = "nonexistent.local";

            // Act
            var result = FormatWrapper.TryCreateEndpoint(hostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), hostname), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_LogsError_WhenHostnameDoesNotMatchMachineName()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            string hostname = "wrong.local";
            var machineHostname = "correct.local";
            var ipAddresses = new[] { IPAddress.Loopback };

            // Mocking GetHostName to return a different hostname
            var formatMock = new Mock<FormatWrapper>(MockBehavior.Strict);
            formatMock.Setup(f => f.GetHostName()).Returns(machineHostname);
            formatMock.Setup(f => Dns.GetHostAddresses(hostname)).Returns(ipAddresses);

            // Act
            var result = FormatWrapper.TryCreateEndpoint(hostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), hostname, machineHostname), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_LogsError_WhenNoReachableIpAddressesFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            string hostname = "unreachable.local";
            var ipAddresses = new[] { IPAddress.Loopback };

            // Mocking TryConnect to always return false
            var formatMock = new Mock<FormatWrapper>(MockBehavior.Strict);
            formatMock.Setup(f => Dns.GetHostAddresses(hostname)).Returns(ipAddresses);
            formatMock.Setup(f => f.TryConnect(It.IsAny<IPEndPoint>())).Returns(false);

            // Act
            var result = FormatWrapper.TryCreateEndpoint(hostname, 8080, true, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), hostname), Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            string hostname = "exception.local";

            // Mocking Dns.GetHostAddresses to throw an exception
            var formatMock = new Mock<FormatWrapper>(MockBehavior.Strict);
            formatMock.Setup(f => Dns.GetHostAddresses(hostname)).Throws(new Exception("Test exception"));

            // Act
            var result = FormatWrapper.TryCreateEndpoint(hostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<string>(), hostname), Times.Once);
            Assert.Null(result);
        }
    }

    public static class FormatWrapper
    {
        public static EndPoint[] TryCreateEndpoint(string singleAddressOrHostname, int port, bool tryConnect = false, ILogger logger = null)
        {
            return Garnet.common.Format.TryCreateEndpoint(singleAddressOrHostname, port, tryConnect, logger);
        }

        public static bool TryConnect(IPEndPoint endpoint)
        {
            return Garnet.common.Format.TryConnect(endpoint);
        }

        public static string GetHostName()
        {
            return Garnet.common.Format.GetHostName();
        }
    }
}
