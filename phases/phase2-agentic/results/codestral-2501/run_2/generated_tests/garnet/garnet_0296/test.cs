using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

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
                x => x.LogError(
                    "Provided hostname does not much acquired machine name {addressOrHostname} {machineHostname}!",
                    It.IsAny<object[]>()),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_NoIPAddresses_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var hostnameWithNoIP = "hostnamewithnoip";

            // Act
            var result = await Format.TryCreateEndpointAsync(hostnameWithNoIP, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "No IP address found for hostname:{hostname}",
                    It.IsAny<object[]>()),
                Times.Once);
            Assert.Null(result);
        }

        [Fact]
        public async Task TryCreateEndpointAsync_Exception_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exceptionHostname = "exceptionhostname";

            // Act
            var result = await Format.TryCreateEndpointAsync(exceptionHostname, 8080, false, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    "Error while trying to resolve hostname: {exMessage} [{hostname}]",
                    It.IsAny<object[]>()),
                Times.Once);
            Assert.Null(result);
        }
    }
}
