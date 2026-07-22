using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;
using System;

namespace Garnet.common.Tests
{
    public class FormatTests
    {
        [Fact]
        public async Task TryCreateEndpointAsync_InvalidHostname_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var invalidHostname = "invalidhostname";

            // Act
            var result = await Format.TryCreateEndpointAsync(invalidHostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    "No IP address found for hostname:{hostname}",
                    It.IsAny<object[]>()),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_HostnameDoesNotMatchMachineName_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "somehostname";

            // Act
            var result = await Format.TryCreateEndpointAsync(hostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    "Provided hostname does not much acquired machine name {addressOrHostname} {machineHostname}!",
                    It.IsAny<object[]>()),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_Exception_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostname = "somehostname";

            // Act
            var result = await Format.TryCreateEndpointAsync(hostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    "Error while trying to resolve hostname: {exMessage} [{hostname}]",
                    It.IsAny<object[]>()),
                Times.Once);
            Assert.Null(result);
        }
    }
}
