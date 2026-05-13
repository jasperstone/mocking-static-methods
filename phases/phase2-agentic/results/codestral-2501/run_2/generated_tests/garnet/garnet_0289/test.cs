using System;
using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        [Fact]
        public void TryCreateEndpoint_InvalidHostname_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var invalidHostname = "invalidhostname";

            // Act
            var result = Format.TryCreateEndpoint(invalidHostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No IP address found for hostname")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_HostnameDoesNotMatchMachineName_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "somehostname";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Provided hostname does not much acquired machine name")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_NoReachableIPAddress_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "somehostname";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 8080, true, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No reachable IP address found for hostname")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public void TryCreateEndpoint_ExceptionDuringResolution_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "somehostname";

            // Act
            var result = Format.TryCreateEndpoint(hostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error while trying to resolve hostname")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.Null(result);
        }
    }
}
